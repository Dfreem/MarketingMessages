using MarketingMessages.Data;
using MarketingMessages.Hubs;
using MarketingMessages.Repository;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketingMessages.Shared.Extensions;
using MarketingMessages.Shared;

namespace MarketingMessages.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EngagementController : ControllerBase
{
    IDbContextFactory<MarketingMessagesContext> _dbFactory;
    UserManager<ApplicationUser> _userManager;
    ILogger<EngagementController> _logger;
    IHubContext<NotificationHub, INotificationClient> _notifications;
    AnalyticsRepository _analyticsRepo;
    //NotificationHub _notifications;


    public EngagementController(IDbContextFactory<MarketingMessagesContext> dbFactory,
                                UserManager<ApplicationUser> userManager,
                                IHubContext<NotificationHub, INotificationClient> notifications,
                                AnalyticsRepository analyticsRepository,
                                ILogger<EngagementController> logger)
    {
        _dbFactory = dbFactory;
        _userManager = userManager;
        _logger = logger;
        _notifications = notifications;
        _analyticsRepo = analyticsRepository;
    }


    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> EventsWebhook([FromBody] Shared.Models.EmailEvent[] payload)
    {
        var db = await _dbFactory.CreateDbContextAsync();
        //payload = payload.Where(p => !db.EmailEvents.Any(e => e.SgEventId == p.SgEventId)).ToArray();
        payload.ToList().ForEach(p => p.ReceivedOn = DateTime.Now);
        db.EmailEvents.AddRange(payload);
        try
        {
            // Tell Sendgrid all is ok before continuing
            Response.StatusCode = 200;
            await Response.CompleteAsync();

            db.SaveChanges();

            var campaignIds = payload.Select(p => p.CampaignId).ToList();
            var campaigns = db.Campaigns.Where(c => campaignIds.Contains(c.CampaignId)).ToList();
            var userIds = campaigns.ToDictionary(c => c.CampaignId, c => c.CreatedBy);
            var models = campaigns.Select(c => c.ToDetailsDisplay()).ToList();
            foreach (var m in models)
            {
                m.Stats = await db.GetCampaignStatsAsync(m.CampaignId);
                await _notifications.Clients.User(userIds[m.CampaignId]).CampaignStatusChanged(m);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving webhook events");
            return StatusCode(500, ex);
        }
    }

    [AllowAnonymous]
    [HttpGet("webhook")]
    public async Task<IActionResult> EventWebhook(string eventType = WebhookEventType.Open, int? campaignId = null, int? contactId = null, string? category = null, string? url = null)
    {
        var db = await _dbFactory.CreateDbContextAsync();
        try
        {

            Contact? contact = null;
            if(contactId is not null)
                contact = db.Contacts.FirstOrDefault(c => c.ContactId == contactId);
            EmailEvent emailEvent = new()
            {
                CampaignId = campaignId??0,
                ContactId = contactId??0,
                Category = category,
                Email = contact?.ContactEmail ?? "",
                Event = eventType,
                Url = url,
                ReceivedOn = DateTime.Now,
                Timestamp = DateTime.Now.Ticks
            };
            db.EmailEvents.Add(emailEvent);
            db.SaveChanges();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured during a webhook trigger\n{ex}", ex);
            return StatusCode(500, ex);
        }

    }
}

