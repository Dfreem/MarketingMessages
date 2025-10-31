using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MarketingMessages.Data;
using MarketingMessages.Repository;
using MarketingMessages.Services;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Extensions;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingMessages.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CampaignsController : BaseController
    {
        private readonly MarketingMessagesContext _context;
        AudienceRepository _audienceRepository;
        AnalyticsRepository _analyticsRepository;
        ILogger<CampaignsController> _logger;
        EmailRepository _emailRepository;
        SuppressionGroupService _suppressionGroupService;

        public CampaignsController(MarketingMessagesContext context,
                                   UserManager<ApplicationUser> userManager,
                                   ILogger<CampaignsController> logger,
                                   AnalyticsRepository analytics,
                                   EmailRepository emailRepo,
                                   SuppressionGroupService suppressionGroupService,
                                   AudienceRepository audienceRepo) : base(userManager)
        {
            _context = context;
            _logger = logger;
            _audienceRepository = audienceRepo;
            _analyticsRepository = analytics;
            _emailRepository = emailRepo;
            _suppressionGroupService = suppressionGroupService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CampaignDetailModel>>> GetUserCampaigns()
        {
            var userId = GetUserId();
            var campaigns = await _context.Campaigns
                .Include(c => c.EmailContent)
                .AsSplitQuery()
                .Include(c => c.Sender)
                .Where(c => c.CreatedBy == userId && !c.Deleted)
                .Select(c => c.ToDetailsDisplay()).ToListAsync();
            foreach (var campaign in campaigns)
            {
                campaign.Contacts = _audienceRepository.ExecuteQueryForList(campaign.AudienceId).Select(s => s.ToDisplayModel()).ToList();
            }
            return Ok(campaigns);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CampaignDetailModel>> GetCampaignDetailsAsync(int id)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .Include(c => c.EmailContent)
                    .AsSplitQuery()
                    .Include(c => c.Sender)
                    .FirstOrDefaultAsync(c => c.CampaignId == id && !c.Deleted);


                if (campaign == null)
                {
                    return NotFound();
                }

                var contacts = _audienceRepository.ExecuteQueryForList(campaign.AudienceId);
                var stats = await _analyticsRepository.GetCampaignStatsAsync(id);
                var unsubscribeGroup = _suppressionGroupService.GetGroupByAudienceId(campaign.AudienceId);
                CampaignDetailModel model = new()
                {
                    AudienceId = campaign.AudienceId,
                    CampaignId = campaign.CampaignId,
                    Contacts = contacts.Select(c => new ContactModel(c)).ToList(),
                    Content = campaign.EmailContent,
                    CreatedOn = campaign.CreatedOn,
                    UnsubscribeGroup = unsubscribeGroup?.ToDisplayModel(),
                    EndDate = campaign.EndDate,
                    IsComplete = campaign.IsComplete,
                    IsEnabled = campaign.IsEnabled,
                    //IsRecurring = campaign.IsRecurring,
                    IsStarted = campaign.IsStarted,
                    //JobType = campaign.JobType,
                    Subject = campaign.Subject,
                    Name = campaign.Name,
                    NextRunDate = campaign.NextExecution,
                    StartDate = campaign.StartDate,
                    From = campaign.Sender.ReplyTo,
                    Stats = stats
                };
                model.Contacts = _audienceRepository.ExecuteQueryForList(campaign.AudienceId).Select(c => new ContactModel(c)).ToList();
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCampaign(CampaignDetailModel campaign)
        {
            try
            {

                var userId = GetUserId();
                if (campaign.CampaignId == 0)
                {
                    if (_context.Campaigns.Any(c => c.Subject == campaign.Subject && c.CreatedBy == userId))
                    {
                        _logger.LogWarning("Attempted to add duplicate subject to Campaigns, {subject}", campaign.Subject);
                        return StatusCode(500, $"Attempted to add duplicate subject to Campaigns, {campaign.Subject}");
                    }

                    var adding = campaign.ToCampaign(userId);
                    _context.Campaigns.Add(adding);
                }
                else
                {
                    var update = await _context.Campaigns.FindAsync(campaign.CampaignId);
                    if (update is null)
                    {
                        update = campaign.ToCampaign(userId);
                        _context.Campaigns.Add(update);
                    }
                    else
                        update.UpdateWith(campaign);
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex);
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            campaign.Deleted = true;
            campaign.IsEnabled = false;
            //_context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("test-campaign/{campaignId}/{email}")]
        public async Task<IActionResult> TestCampaignEmail(int campaignId, string email)
        {
            var userId = GetUserId();
            var campaign = await _context.Campaigns.Include(c => c.EmailContent)
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.CreatedBy == userId);
            if (campaign is null)
            {
                _logger.LogWarning("Unable to find campaign with id {campaignId} for user with id {userId}", campaignId, email);
                return StatusCode(500, "Unable to resolve campaing");
            }

            var contact = _context.Contacts.FirstOrDefault(c => c.ContactEmail == email && c.CreatedBy == userId);
            contact ??= new()
            {
                ContactEmail = email,
                CreatedBy = userId
            };
            var sender = _context.Senders.FirstOrDefault(s => s.CreatedBy == userId);
            if (sender is null)
            {
                return StatusCode(500, "Unable to resolve sender for current user.");
            }
            var subNames = campaign.EmailContent.TemplatePropertyNames.Split(',').ToList();

            var result = _emailRepository.SendTestEmail(campaign.EmailContent.HtmlContent, campaign.Subject, subNames, sender, contact);
            if (result.Success)
            {
                return Ok();
            }
            return StatusCode(500, result.Error);
        }

        [HttpPost("save")]
        public async Task<ActionResult<CampaignDetailModel>> PostCampaign([FromBody] CampaignDetailModel campaign)
        {
            try
            {

                var userId = GetUserId();
                if (campaign.CampaignId == 0)
                {
                    if (_context.Campaigns.Any(c => c.Subject == campaign.Subject && c.CreatedBy == userId))
                    {
                        _logger.LogWarning("Attempted to add duplicate subject to Campaigns, {subject}", campaign.Subject);
                        return StatusCode(500, $"Attempted to add duplicate subject to Campaigns, {campaign.Subject}");
                    }

                    var adding = campaign.ToCampaign(userId);
                    _context.Campaigns.Add(adding);
                    _context.SaveChanges();
                    _context.SaveChanges();
                    return Ok(adding);
                }
                else
                {
                    if (campaign.Content is not null)
                    {
                        _context.Contents.Update(campaign.Content);

                    }
                    var update = await _context.Campaigns
                        .Include(c => c.EmailContent)
                        .FirstOrDefaultAsync(c => c.CampaignId == campaign.CampaignId);
                    if (update is null)
                    {
                        update = campaign.ToCampaign(userId);
                        _context.Campaigns.Add(update);
                    }
                    else
                        update.UpdateWith(campaign);

                    _context.SaveChanges();
                    return Ok(update.ToDetailsDisplay());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex);
            }

        }

        [HttpPost]
        public ActionResult<CampaignDetailModel> SaveCampaignWizard([FromBody] WizardRequest request)
        {
            try
            {
                var userId = GetUserId();
                var campaign = request.AsCampaign(userId);
                _context.Campaigns.Add(campaign);
                _context.SaveChanges();
                return Ok(campaign.ToDetailsDisplay());
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return StatusCode(500, ex);
            }
        }

        private bool CampaignExists(int id)
        {
            return _context.Campaigns.Any(e => e.CampaignId == id);
        }
    }
}
