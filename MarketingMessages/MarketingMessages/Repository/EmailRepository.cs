
using MimeKit;
using Sender = MarketingMessages.Shared.Models.Sender;
using MarketingMessages.Services;
using MarketingMessages.Data;
using MarketingMessages.Shared.Models;
using MarketingMessages.Shared.DTO;
using System.Threading.Tasks;

namespace MarketingMessages.Repository;

public class EmailRepository
{
    private MarketingMessagesContext _db;
    ILogger<EmailRepository> _logger;
    SmtpService _smtpService;

    public EmailRepository(IDbContextFactory<MarketingMessagesContext> contextFactory,
                           ILogger<EmailRepository> logger,
                           SmtpService smtpService)
    {
        _db = contextFactory.CreateDbContext();
        _logger = logger;
        _smtpService = smtpService;
    }


    public async Task<SuccessResponse> SendEmails(int jobId)
    {
        //var tos = emailRequest.ContactList.Contacts.Select(e => new MailboxAddress(e.ContactEmail, e.ContactFirstName)).ToList();
        var job = _db.Campaigns.Include(j => j.Audience)
            .AsSplitQuery()
            .Include(j => j.EmailContent)
            .AsSplitQuery()
            .Include(j => j.Sender)
            .FirstOrDefault(j => j.CampaignId == jobId);
        if (job is null || job.EmailContent is null)
        {
            _logger.LogError("Unable to find job with id {jobId} in EmailRepository.SendEmails", jobId);
            return new() { Error = $"Unable to find job with id {jobId} in EmailRepository.SendEmails" };
        }
        return await SendEmails(job);
    }

    public async Task<SuccessResponse> SendEmails(Campaign job)
    {
        //var tos = emailRequest.ContactList.Contacts.Select(e => new MailboxAddress(e.ContactEmail, e.ContactFirstName)).ToList();
        if (job is null || job.EmailContent is null)
        {
            _logger.LogError("Attempted to run job with some missing essential fields.");
            return new() { Error = "Attempted to run job with some missing essential fields." };
        }
        if (job.Sender is null)
        {
            _logger.LogError("No sender was attached to this job. JobId: {jobId}", job.CampaignId);
            return new() { Error = $"Unable to find a sender with id {job.SenderId}" };
        }

        // Send Smtp Email
        var response = await _smtpService.SendEmails(job); // TODO Add attachments
        string? result = response.Success ? response.Message : response.Error;

        // Log Email send attempt
        SentLog sentLog = new()
        {
            Body = job.EmailContent.TextContent,
            CreatedBy = job.CreatedBy,
            CreatedOn = DateTime.Now,
            EmailContentId = job.EmailContentId,
            SendListId = job.AudienceId,
            DateSent = DateTime.Now,
            Result = result,
            Subject = job.EmailContent.Name,
            Success = response.Success,
            JobId = job.CampaignId
        };
        _db.SentLogs.Add(sentLog);
        job.IsComplete = true;
        job.IsExecuting = false;
        _db.Campaigns.Update(job);
        _db.SaveChanges();

        return response;
    }
    //public async Task<SuccessResponse> SendEmailsWithSendgrid(Campaign job)
    //{
    //    //var tos = emailRequest.ContactList.Contacts.Select(e => new MailboxAddress(e.ContactEmail, e.ContactFirstName)).ToList();
    //    if (job is null || job.EmailContent is null)
    //    {
    //        _logger.LogError("Attempted to run job with some missing essential fields.");
    //        return new() { Error = "Attempted to run job with some missing essential fields." };
    //    }
    //    if (job.Sender is null)
    //    {
    //        _logger.LogError("No sender was attached to this job. JobId: {jobId}", job.CampaignId);
    //        return new() { Error = $"Unable to find a sender with id {job.SenderId}" };
    //    }

    //    MailboxAddress from = new(job.Sender.Name, job.Sender.ReplyTo);
    //    var contacts = _sendlistRepo.ExecuteQueryForList(job.AudienceId);
    //    Dictionary<string, string[]> subs = [];
    //    Dictionary<string, string> uniqueValues = [];
    //    if (!string.IsNullOrEmpty(job.EmailContent.TemplatePropertyNames))
    //    {

    //        var subNames = job.EmailContent.TemplatePropertyNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    //        subNames.Remove("ContactId");
    //        foreach (var contact in contacts)
    //        {
    //            foreach (var property in subNames)
    //            {
    //                string propName = property.Replace("#", "");
    //                string? subValue = contact.GetType().GetProperty(propName)?.GetValue(contact)?.ToString();
    //                if (string.IsNullOrEmpty(subValue))
    //                    continue;
    //                if (!subs.TryGetValue(property, out var v))
    //                {
    //                    v = [subValue];
    //                    subs.Add(property, v);
    //                }
    //                else
    //                {
    //                    subs[property] = [.. v, subValue];
    //                }

    //            }
    //            if (!subs.TryGetValue("#ContactId#", out var subValues))
    //            {
    //                subValues = [contact.ContactId.ToString()];
    //                subs.Add("#ContactId#", subValues);
    //            }
    //            else
    //            {
    //                subs["#ContactId#"] = [.. subValues, contact.ContactId.ToString()];
    //            }
    //        }
    //    }

    //    uniqueValues.Add("contact_id", "#ContactId#");
    //    uniqueValues.Add("campaign_id", job.CampaignId.ToString());

    //    // Send Smtp Email
    //    var response = await _smtpService.SendEmail(contacts,
    //                                          from,
    //                                          job.Subject,
    //                                          job.EmailContent.HtmlContent,
    //                                          unsubscribeGroup: job.Audience?.UnsubscribeGroup,
    //                                          subs: subs,
    //                                          uniqueArgs: uniqueValues); // TODO Add attachments
    //    string? result = response.Success ? response.Message : response.Error;

    //    result += "\n" + "Unique Arguments:  " + string.Join(", ", uniqueValues);

    //    // Log Email send attempt
    //    SentLog sentLog = new()
    //    {
    //        Body = job.EmailContent.TextContent,
    //        CreatedBy = job.CreatedBy,
    //        CreatedOn = DateTime.Now,
    //        EmailContentId = job.EmailContentId,
    //        SendListId = job.AudienceId,
    //        DateSent = DateTime.Now,
    //        Result = result,
    //        Subject = job.EmailContent.Name,
    //        Success = response.Success,
    //        JobId = job.CampaignId
    //    };
    //    _db.SentLogs.Add(sentLog);
    //    job.IsComplete = true;
    //    job.IsExecuting = false;
    //    _db.Campaigns.Update(job);
    //    _db.SaveChanges();

    //    return response;
    //}

    public SuccessResponse SendTestEmail(string body, string subject, IEnumerable<string> subs, Sender from, Contact to)
    {
        // Send Smtp Email
        return _smtpService.SendTestEmail(from, to, body, subject, subs); // TODO Add attachments
    }


    // The webhook sends us this info allready
    //public async Task<EngagementUrl> GenerateEnhancementUri(int contactId, int campaignId, string userId)
    //{
    //    UriBuilder builder = new();
    //    builder.UserName = contactId.ToString();
    //    EngagementUrl engagement = new()
    //    {
    //        UserId = userId,
    //        CampaignId = campaignId,
    //        ContactId = contactId,
    //        Url = builder.ToString(),
    //    };
    //    using var db = await _db.CreateDbContextAsync();
    //    db.EngagementUrls.Add(engagement);
    //    db.SaveChanges();
    //    return engagement;
    //}
}
