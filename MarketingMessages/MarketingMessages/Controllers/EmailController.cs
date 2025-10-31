using Ganss.Xss;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MarketingMessages.Shared.Enums;
using System.Threading.Tasks.Dataflow;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using MarketingMessages.Hubs;
using MarketingMessages.Repository;
using MarketingMessages.Data;
using MarketingMessages.Shared.Models;
using MarketingMessages.Shared.DTO.V1;
using MarketingMessages.Shared.Extensions;
using System.Diagnostics;

namespace MarketingMessages.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EmailController : BaseController
{
    private IDbContextFactory<MarketingMessagesContext> _db;
    ILogger<EmailController> _logger;
    EmailRepository _emailRepo;
    SmtpService _smtpService;
    IHubContext<NotificationHub, INotificationClient> _notificationHub;

    public EmailController(IDbContextFactory<MarketingMessagesContext> contextFactory, EmailRepository emailRepo, UserManager<ApplicationUser> userManager, ILogger<EmailController> logger, AudienceRepository sendListRepository, IHubContext<NotificationHub, INotificationClient> notificationsHub, SmtpService smtpService) : base(userManager)
    {
        _db = contextFactory;
        _logger = logger;
        _emailRepo = emailRepo;
        _notificationHub = notificationsHub;
        _smtpService = smtpService;
    }



    [HttpPost("schedule-job")]
    public async Task<IActionResult> ScheduleEmailAsync([FromBody] WizardRequest request)
    {
        try
        {
            var userId = GetUserId();
            using var db = _db.CreateDbContext();
            if (db.Campaigns.Any(c => c.Subject == request.Subject && c.CreatedBy == userId))
            {
                _logger.LogWarning("This subject is being used already, please change the subject of the email.");
                return BadRequest("This subject is being used already, please change the subject of the email.");

            }
            var sender = await db.Senders.FirstOrDefaultAsync(s => s.CreatedBy == userId);
            if (userId is null || sender is null)
                return StatusCode(500, "Unable to resolve user id for logged in user");
            // TODO left off here
            var audience = await db.Audiences.FirstOrDefaultAsync(l => l.Id == request.AudienceId);
            if (audience is null)
            {
                _logger.LogError("The cantact list with id {id} does not exist", request.AudienceId);
                return StatusCode(500, $"The cantact list with id {request.AudienceId} does not exist");
            }
            Content? content = db.Contents.FirstOrDefault(e => e.ContentId == request.ContentId);
            if (request.ContentId == 0 || content is null)
            {
                content = new()
                {
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    HtmlContent = request.EditorContent.HtmlContent ?? "",
                    TextContent = request.EditorContent.TextContent,
                    TemplatePropertyNames = string.Join(',', request.SubNames),
                    Name = request.Subject,
                };
                db.Contents.Add(content);

                // save to get ContentId
                db.SaveChanges();
                request.ContentId = content.ContentId;
            }
            content.HtmlContent = request.EditorContent.HtmlContent ?? "";
            content.TextContent = request.EditorContent.TextContent;
            content.TemplatePropertyNames = string.Join(',', request.SubNames);
            if (content is not null && request.SubNames.Count != 0 && request.SubNames.Any(n => !content.TemplatePropertyNames.Contains(n)))
                content.TemplatePropertyNames = string.Join(",", request.SubNames);

            // add content to job, then save
            Campaign scheduledEmail = new()
            {
                CreatedOn = DateTime.Now,
                CreatedBy = userId,
                StartDate = request.StartTime,
                //EndDate = request.EndTime,
                //JobType = request.JobType.ToString(),
                //IsRecurring = request.Recurring,
                //FrequencyUnit = Enum.GetName<IntervalUnit>(request.FrequencyUnit) ?? "",
                //FrequencyInterval = request.FrequencyInterval,
                //NextExecution = request.StartTime,
                SenderId = sender.SenderId,
                EmailContentId = content?.ContentId ?? 0,
                AudienceId = audience.Id,
                //TotalJobLimit = request.TotalToSend,
                Subject = request.Subject,
                Name = request.DocumentTitle,
                //IsEnabled = true, // default to enabled
            };
            db.Campaigns.Add(scheduledEmail);

            // save to get JobId
            db.SaveChanges();
            if (request.SendNow)
            {
                var emailResponse = await _emailRepo.SendEmails(scheduledEmail);
                if (!emailResponse.Success)
                {
                    return StatusCode(500, emailResponse.Error);
                }
            }

            // send the new job id back to the client
            request.JobId = scheduledEmail.CampaignId;
            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while attempting to schedule emails\n{ex}", ex);
            return StatusCode(500, $"An error occurred while attempting to schedule emails\n{ex}");
        }
    }

    [HttpGet("jobs")]
    public ActionResult<List<EmailSchedule>> GetScheduledEmailsAsync()
    {

        var userId = GetUserId();
        if (userId == null) return StatusCode(500, "Unable to resolve user id");
        using var db = _db.CreateDbContext();
        var emailJobs = db.Campaigns.Include(j => j.EmailContent).Where(j => j.CreatedBy == userId && j.EmailContent != null);
        var jobIds = emailJobs.Select(j => j.CampaignId).ToList();
        var sentLogs = db.SentLogs.Where(l => jobIds.Contains(l.JobId));
        var results = emailJobs.Select(j => new EmailSchedule()
        {
            TextContent = j.EmailContent!.TextContent ?? "",
            HtmlContent = j.EmailContent.HtmlContent ?? "",
            EmailSubject = j.Subject,
            //SubNames = j.EmailContent.TemplatePropertyNames.Split(',').ToList()
            StartDate = j.StartDate,
            //EndDate = j.EndDate,
            IsSending = j.IsExecuting,
            IsComplete = j.IsComplete,
            //TotalToSend = j.TotalJobLimit,
            SenderId = j.SenderId,
            //TotalSent = j.JobsCompleted,
            //JobType = String.IsNullOrEmpty(j.JobType) ? EmailJobType.OneOff : Enum.Parse<EmailJobType>(j.JobType),
            Logs = sentLogs.Where(l => l.EmailContentId == j.EmailContentId).ToList(),
            JobId = j.CampaignId,
            ListId = j.CampaignId,
            //Recurring = j.JobType != null && j.JobType.ToLower() == EmailJobType.Recurring.ToString().ToLower(),
            //FrequencyInterval = j.FrequencyInterval,
            //FrequencyUnit = Enum.Parse<IntervalUnit>(j.FrequencyUnit),
            CanDelete = !sentLogs.Any(l => l.JobId == j.CampaignId) && !j.IsExecuting && !j.IsStarted,
        }).ToList();
        return results;
    }

    [HttpGet("message-content/{contentId?}")]
    public async Task<ActionResult<EmailContentResponse>> GetEmailContentAsync(int? contentId)
    {
        try
        {
            var userId = GetUserId();
            if (userId is null)
            {
                _logger.LogError("Unable resolve user while attempting to update contact in {MethodName}", nameof(GetEmailContentAsync));
                return StatusCode(500, $"Unable resolve user while attempting to update contact in {nameof(GetEmailContentAsync)}");
            }

            using var db = _db.CreateDbContext();
            var htmlContent = await db.Contents.FindAsync(contentId);
            var response = new EmailContentResponse()
            {
                HtmlContent = htmlContent?.TextContent ?? "",
                ContentId = htmlContent?.ContentId ?? 0,
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while attempting to get html content with id {htmlId}\n{ex}", contentId, ex);
            return StatusCode(500, ex);
        }
    }

    [HttpDelete("delete-content/{contentId}")]
    public async Task<IActionResult> DeleteEmailContentAsync(int contentId)
    {
        try
        {

            using var db = _db.CreateDbContext();
            var toDelete = await db.Contents.FirstOrDefaultAsync(c => c.ContentId == contentId);
            if (toDelete is not null)
            {
                db.Contents.Remove(toDelete);
                db.SaveChanges();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while attempting to delete email contents with id {contentId}\n{ex}", contentId, ex);
            return StatusCode(500, $"An error occurred while attempting to delete email contents with id {contentId}\n{ex.Message}");
        }
    }

    [HttpDelete("delete-job/{jobId}")]
    public async Task<IActionResult> DeleteJobAsync(int jobId)
    {
        try
        {
            var userId = GetUserId();
            if (userId is null)
            {
                _logger.LogError("Unable resolve user while attempting to update contact in {MethodName}", nameof(GetEmailContentAsync));
                return StatusCode(500, $"Unable resolve user while attempting to update contact in {nameof(GetEmailContentAsync)}");
            }

            using var db = _db.CreateDbContext();
            var emailJob = await db.Campaigns.FindAsync(jobId);
            if (emailJob is null)
            {
                _logger.LogWarning("Unable to find email job with id {emailJobId} while attempting to delete", jobId);
                return StatusCode(500, $"Unable to find email job with id {jobId} while attempting to delete");
            }
            if (emailJob.Deleted)
            {
                string message = $"Job with id {emailJob.CampaignId} is deleted";
                _logger.LogWarning(message);
                return BadRequest(message);
            }
            emailJob.Deleted = true;
            //db.Campaigns.Remove(emailJob);
            db.SaveChanges();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while attempting to delete email job with id {jobId}\n{ex}", jobId, ex);
            return StatusCode(500, ex);
        }
    }

    [HttpGet("test-email/{campaignId}")]
    public IActionResult SendTestEmail(int campaignId)
    {
        var userId = GetUserId();
        var db = _db.CreateDbContext();

        var campaign = db.Campaigns
            .Include(c => c.Audience)
            .AsSplitQuery()
            .Include(c => c.EmailContent)
            .AsSplitQuery()
            .Include(c => c.Sender)
            .FirstOrDefault(c => c.CampaignId == campaignId);
        var contact = campaign?.Sender.AsContact();
        contact ??= db.Senders.FirstOrDefault(s => s.CreatedBy == userId)?.AsContact();

        if (contact == null)
        {
            return BadRequest("You do not currently have a Sender registered. Please create a sender."); // TODO add link to Sender management when after it has been imlemented
        }
        Debug.Assert(campaign != null);
        try
        {
            var result = _smtpService.SendEmail(campaign, contact);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("test-email")]
    public ActionResult SendTestEmail([FromBody] HtmlEditorContent editorContent)
    {
        var userId = GetUserId();
        var db = _db.CreateDbContext();

        var sender = db.Senders.FirstOrDefault(s => s.CreatedBy == userId);
        if (sender is null)
            return StatusCode(500, "Please create a Sender.");

        var contact = sender.AsContact();
        var result = _emailRepo.SendTestEmail(editorContent.HtmlContent, editorContent.Subject, editorContent.TemplateVariableNames, sender, contact);
        if (result.Success)
            return Ok();
        return StatusCode(500, result.Error);
    }

}
