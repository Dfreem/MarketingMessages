using Microsoft.Extensions.Options;

using MimeKit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using AngleSharp.Css.Parser;
using SendGrid;
using MarketingMessages.Shared.Models;
using MarketingMessages.Shared.DTO.V1;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Repository;
using Sender = MarketingMessages.Shared.Models.Sender;
using Serilog;
using MarketingMessages.Shared;

namespace MarketingMessages.Services;

public class SmtpService
{
    private readonly SmtpOptions _smtpOptions;
    private IConfiguration _config;
    private readonly ILogger<SmtpService> _logger;
    private SuppressionGroupService _supressionsService;
    private AudienceRepository _audiencRepo;
    private string _baseUrl;

    public SmtpService(IOptions<SmtpOptions> smtpOptions,
                       ILogger<SmtpService> logger,
                       IConfiguration config,
                       AudienceRepository audienceRepo,
                       SuppressionGroupService groupService)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
        _config = config;
        _supressionsService = groupService;
        _audiencRepo = audienceRepo;
        _baseUrl = config["HttpBaseUrl"] ?? "";
    }

    public async Task<SuccessResponse> SendEmails(Campaign campaign)
    {

        using SmtpClient client = new();
        if (string.IsNullOrEmpty(_smtpOptions.Server))
        {
            _smtpOptions.Server = _config["SendGridSMTPServer"] ?? throw new ArgumentNullException("SendGridSMTPServer was not found in configuration");
            _smtpOptions.Port = Convert.ToInt32(_config["SendGridSMTPPort"]);
            _smtpOptions.Username = _config["SendGridSMTPUsername"] ?? throw new ArgumentNullException("SendGridSMTPUsername was not found in configuration");
            _smtpOptions.Password = _config["SendGridSMTPPassword"] ?? throw new ArgumentNullException("SendGridSMTPPassword was not found in configuration");
        }

        List<string> errors = [];
        List<string> messages = [];
        var audience = _audiencRepo.ExecuteQueryForList(campaign.AudienceId);
        await Task.Run(() =>
        {
            foreach (var contact in audience)
            {
                var message = SendEmail(campaign, contact);
                if (message.Success)
                    messages.Add(message.Message ?? contact.ContactEmail);
                else
                    errors.Add(message.Error ?? contact.ContactEmail);
            }
        });
        return new() { Message = String.Join("\n", messages), Success = true, Error = String.Join("\n", errors) };
    }


    public SuccessResponse SendEmail(Campaign campaign, Contact contact)
    {

        using SmtpClient client = new();
        try
        {
            if (string.IsNullOrEmpty(_smtpOptions.Server))
            {
                _smtpOptions.Server = _config["SendGridSMTPServer"] ?? throw new ArgumentNullException("SendGridSMTPServer was not found in configuration");
                _smtpOptions.Port = Convert.ToInt32(_config["SendGridSMTPPort"]);
                _smtpOptions.Username = _config["SendGridSMTPUsername"] ?? throw new ArgumentNullException("SendGridSMTPUsername was not found in configuration");
                _smtpOptions.Password = _config["SendGridSMTPPassword"] ?? throw new ArgumentNullException("SendGridSMTPPassword was not found in configuration");
            }

            MimeMessage message = CreateEmailMessage(contact, campaign);

            client.Connect(_smtpOptions.Server, _smtpOptions.Port, false);
            client.Authenticate(_smtpOptions.Username, _smtpOptions.Password);
            string response = client.Send(message);

            return new() { Message = response, Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
            return new() { Error = ex.Message };
        }
        finally
        {
            client.Disconnect(true);
        }
    }

    public SuccessResponse SendTestEmail(Sender from, Contact to, string body, string subject, IEnumerable<string> subs)
    {
        try
        {

            MailboxAddress fromAddress = new(from.Name, from.ReplyTo);
            MailboxAddress toAddress = new(to.FirstName, to.ContactEmail);
            string result = body.Clone().ToString() ?? "";
            foreach (var replacement in subs)
            {
                var propertyName = replacement.Replace("#", "");
                var propertyValue = typeof(Contact).GetProperty(propertyName)?.GetValue(to);
                if (propertyValue is null)
                    continue;
                result = result.Replace(replacement, propertyValue.ToString());
            }
            MimeMessage message = new(fromAddress, toAddress, subject, result);
            message.To.Add(toAddress);
            message.From.Add(fromAddress);
            message.Subject = subject;
            var builder = new BodyBuilder()
            {
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();
            // No headers for test emails, they don't count towards analytics

            using SmtpClient client = new SmtpClient();
            client.Connect(_smtpOptions.Server, _smtpOptions.Port, false);
            client.Authenticate(_smtpOptions.Username, _smtpOptions.Password);
            string response = client.Send(message);

            return new() { Message = response, Success = true };
        }
        catch (Exception ex)
        {
            Log.Error("Error occurred while sending test email\n{ex}", ex);
            return new() { Error = ex.Message };
        }

    }
    private MimeMessage CreateEmailMessage(Contact contact, Campaign campaign)
    {
        var sender = campaign.Sender;
        MailboxAddress from = new(sender.Name, sender.ReplyTo);
        MailboxAddress to = new(contact.FirstName, contact.ContactEmail);
        string body = campaign.EmailContent.ReplaceSubstitutionTags(contact);

        body += $"<img hidden width='1' height='1' src='{_baseUrl}/api/Engagement/webhook" +
            $"?eventType={WebhookEventType.Open}" +
            $"&campaignId={campaign.CampaignId}" +
            $"&contactId={contact.ContactId}" +
            $"&category='default' />";
        MimeMessage message = new();
        message.To.Add(to);
        message.From.Add(from);
        message.Subject = campaign.Subject;
        var builder = new BodyBuilder
        {
            HtmlBody = body
        };

        message.Body = builder.ToMessageBody();

        //if (unsubscribeGroup is not null && unsubscribeGroup != 0)
        //{

        var smtpHeader = new
        {
            campaign_id = campaign.CampaignId,
            contact_id = contact.ContactId,
        };
        var jsonHeader = JsonConvert.SerializeObject(smtpHeader);
        message.Headers.Add("X-SMTPAPI", jsonHeader);
        //}
        //else
        //{
        //    var smptApiHeader = new
        //    {
        //        to = tos.Select(t => t.ContactEmail).ToArray(),
        //        sub = subs,
        //        unique_args = uniqueArgs
        //    };
        //    var jsonHeader = JsonConvert.SerializeObject(smptApiHeader);
        //    message.Headers.Add("X-SMTPAPI", jsonHeader);
        //}

        return message;
    }

}


public class EmailAttachment
{
    public string FileName { get; set; } = default!;
    public Stream ContentStream { get; set; } = default!;

    public string MediaType { get; set; } = "image/png";


}

