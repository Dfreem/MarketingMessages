using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MarketingMessages.Repository;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MarketingMessages.Data;

using static MarketingMessages.Client.Pages.Home;

namespace MarketingMessages.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        AnalyticsRepository _repo;
        ILogger<AnalyticsController> _logger;
        UserManager<ApplicationUser> _userManager;

        public AnalyticsController(AnalyticsRepository repo, ILogger<AnalyticsController> logger, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _logger = logger;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            string userId = _userManager.GetUserId(User) ?? "";
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Unable to resolve user id in `{pathBase}/{path}", Request.PathBase, Request.Path);
                throw new Exception("Unable to resolve user id in `/api/SendList/");
            }
            return userId;
        }

        [HttpGet("general/{start?}/{end?}")]
        public ActionResult<StatisticsModel> GetStatistics(DateTime? start = null, DateTime? end = null)
        {
            string userId = GetUserId();
            var stats = _repo.GetStatistics(userId, start, end);
            return Ok(stats);
        }

        [HttpGet("line-chart/{start?}/{end?}")]
        public ActionResult<StatisticsModel> GetStatistics(DateTime? start = null, DateTime? end = null)
        {
            string userId = GetUserId();
            var stats = _repo.GetStatistics(userId, start, end);
            return Ok(stats);
        }

        [HttpGet("campaign/{campaignId}")]
        public async Task<ActionResult<StatisticsModel>> GetCampaignStatsAsync(int campaignId)
        {
            try
            {

                var stats = _repo.GetCampaignStatsAsync(campaignId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                Log.Error("{EX}", ex);
                return BadRequest("Unable to find statistics for campaign");
            }
        }
    }
}
