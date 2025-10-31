using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

using MarketingMessages.Shared.DTO;
using MimeKit;
using Sender = MarketingMessages.Shared.Models.Sender;
using MarketingMessages.Repository;
using MarketingMessages.Data;
using MarketingMessages.Shared.Models;
using MarketingMessages.Shared.Extensions;

namespace MarketingMessages.Workers
{
    public class EmailQueueWorker : BackgroundService
    {
        private readonly ILogger<EmailQueueWorker> _logger;
        private IDbContextFactory<MarketingMessagesContext> _dbFactory;
        private static ConcurrentQueue<Campaign> _queue = new();
        private int _timerInterval = 1;
        private EmailRepository _emailRepo;
        private AudienceRepository _sendListRepo;
        public static int TotalQueueRunners { get; set; }
        const int _concurrencyLimit = 4;

        public EmailQueueWorker(ILogger<EmailQueueWorker> logger, IServiceProvider services)
        {
            _logger = logger;
            using var scope = services.CreateScope();
            _dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MarketingMessagesContext>>();
            _emailRepo = scope.ServiceProvider.GetRequiredService<EmailRepository>();
            _sendListRepo = scope.ServiceProvider.GetRequiredService<AudienceRepository>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            async Task processQueue()
            {

                _logger.LogInformation("Checking for jobs to run");
                using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);

                // getting jobs to run
                // If this service goes offline with jobs in the queue, their status will be IsStarted == true && IsExcecuting == false.
                // In order to restart any jobs waiting in the queue when the service went down, we do not filter out jobs with IsStarted == true yet.
                var jobs = db.Campaigns.Include(j => j.Audience)
                    .AsSplitQuery()
                    .Include(j => j.EmailContent)
                    .AsSplitQuery()
                    .Include(j => j.Sender)
                    .Where(
                        j => j.StartDate != null &&
                        j.StartDate <= DateTime.Now &&
                        j.IsEnabled &&
                        !j.IsComplete &&
                        !j.IsExecuting) // don't start jobs that are already runninng on another thread 
                    .ToList();

                //// filter out recurring jobs that should not run right now
                //jobs = jobs.Where(j => j.IsEnabled && (j.NextExecution <= DateTime.Now || j.NextExecution == null)).ToList();

                if (jobs.Count != 0)
                    _logger.LogInformation("Found jobs to run.\n{jobs}", jobs);

                if (jobs.Count == 0)
                    return; ;
                foreach (var job in jobs)
                {
                    if (job.IsStarted)
                    {
                        // now we check if the job has been started and is currently on the queue.
                        if (_queue.Any(q => q.CampaignId == job.CampaignId))
                        {
                            return;
                        }
                        // if the job has not been enqueue'd, we allow the job to be added to the queue with the rest of the jobs.
                        _logger.LogInformation("Found job that was in the queue when the worker service shut down\n{job}", job.AsString());
                    }

                    try
                    {
                        job.IsStarted = true;
                        db.SaveChanges();
                        _queue.Enqueue(job);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error occurred int Worker.ExecuteAsync\n{ex}", ex);
                    }
                }
                if (!_queue.IsEmpty && TotalQueueRunners < _concurrencyLimit)
                {
                    TotalQueueRunners += 1; // limit total cuncurrently running processes
                    await RunQueueJobs();
                    TotalQueueRunners -= 1;
                }
            }

            // call closure once when initialized
            await processQueue();

            // set timer to run processQueue per _timerInterval
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(_timerInterval));
            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                await processQueue();
            }

        }

        private async Task RunQueueJobs()
        {
            while (_queue.TryDequeue(out Campaign? job))
            {
                if (job.IsExecuting)
                    continue;
                using var db = await _dbFactory.CreateDbContextAsync();
                List<Contact> list = [];

                // set IsExecuting immidiately to prevent other processes from starting the same job at the same time.
                job.IsExecuting = true;
                db.SaveChanges();

                _logger.LogInformation("Processing Job\n{job}", job);

                if (job is null)
                {
                    _logger.LogError("Tried running null job. EmailScheduleService.RunQueueJobs");
                    continue;
                }
                try
                {

                    list = _sendListRepo.ExecuteQueryForList(job.AudienceId);

                    if (list.Count == 0)
                    {
                        _logger.LogError("Unable to find contacts for list with id {ListId}", job.AudienceId);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("JobId: {EmailJobId} \nError occurred in Worker.RunQueueJobs\n{ex}", job.CampaignId, ex);
                    continue;
                }

                // find sender being sent by
                Sender? sender = db.Senders.FirstOrDefault(s => s.SenderId == job.SenderId);
                if (sender is null)
                {
                    _logger.LogError("Unable to find sender with id {SenderId} for list with id {ListId}", job.SenderId, job.AudienceId);
                    continue;
                }


                //// send email
                var response = await _emailRepo.SendEmails(job);
                string result = "";
                if (!response.Success)
                    result = $"There was an error while attempting to send email.\n{response.Error}";
                else
                    result = response.Message ?? "No Response";

                // log email send
                SentLog sentLog = new()
                {
                    Body = job.EmailContent.HtmlContent,
                    CreatedBy = job.CreatedBy,
                    CreatedOn = job.CreatedOn,
                    SendListId = job.AudienceId,
                    DateSent = DateTime.Now,
                    Result = result,
                    EmailContentId = job.EmailContentId,
                    JobId = job.CampaignId,
                    Subject = job.EmailContent.Name,
                    Success = response.Success,
                };
                db.SentLogs.Add(sentLog);

                // log job execution
                var jobLog = new JobLog()
                {
                    //FinishDate = job.EndDate,
                    JobId = job.CampaignId,
                    StartDate = job.StartDate ?? DateTime.Now,
                    Notes = result,
                    Succeeded = response.Success
                };

                try
                {
                    db.JobLogs.Add(jobLog);
                    job.IsExecuting = false;
                    job.IsComplete = true;

                    // if job is finished with no next execution, this also set IsComplete to true
                    //job.SetNextExecution();
                    //job.JobsCompleted += 1;
                    db.Campaigns.Update(job);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError("{ex}", ex);
                }
                _logger.LogInformation("Completing Job \nJobId: {jobId} is finished running.\nComplete: {IsComplete}", job.CampaignId, job.IsComplete);
            }
        }
    }
}
