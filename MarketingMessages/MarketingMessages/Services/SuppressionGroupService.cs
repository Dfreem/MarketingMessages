using MailKit.Net.Smtp;

using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using SendGrid;

using Serilog;

using System.CodeDom;
using System.Diagnostics;

namespace MarketingMessages.Services
{
    /// <summary>
    /// This service is meant to be run on the server only. Sendgrid Suppression or Unsubscribe groups.
    /// </summary>
    public class SuppressionGroupService
    {
        MarketingMessagesContext _db;
        public SuppressionGroupService(IConfiguration config, IDbContextFactory<MarketingMessagesContext> dbContextFactory)
        {
            //var sendgridApiKey = config["SendgridApi:ApiKey"] ?? throw new Exception("Unable to find SendGrid api key in configuration");
            //_sendGrid = new(sendgridApiKey);
            _db = dbContextFactory.CreateDbContext();
        }

        public async Task<List<SuppressionGroup>> GetUserSuppressionGroupsAsync(string userId, bool includeDeleted = false)
        {
            List<SuppressionGroup> suppressionGroups = [];
            if (includeDeleted)
                suppressionGroups = await _db.SuppressionGroups.Where(g => g.CreatedBy == userId).ToListAsync();
            else
                suppressionGroups = await _db.SuppressionGroups.Where(g => g.CreatedBy == userId && !g.Deleted).ToListAsync();
            return suppressionGroups; ;
        }

        public SuppressionGroup? GetGroupByAudienceId(int audienceId)
        {
            var audience = _db.Audiences.Include(a => a.SuppressionGroup).FirstOrDefault(a => a.Id == audienceId);
            if (audience == null || audience.SuppressionGroup == null)
            {
                Log.Warning("unable to find Unsubscribe Group for audience with id {audienceId}", audienceId);
                return null;
            }
            return audience.SuppressionGroup;
        }
    }
}
