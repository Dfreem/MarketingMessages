using BlazorExpress.ChartJS;

using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;

using System.Threading.Tasks;

using static MarketingMessages.Client.Pages.Home;

namespace MarketingMessages.Repository;

public class AnalyticsRepository
{
    private MarketingMessagesContext _db;
    public AnalyticsRepository(IDbContextFactory<MarketingMessagesContext> db)
    {
        _db = db.CreateDbContext();
    }
    public StatisticsModel GetStatistics(string userId, DateTime? start = null, DateTime? end = null)
    {
        var userCampaings = _db.Campaigns.Where(c => c.CreatedBy == userId && c.IsEnabled);
        var campaignIds = userCampaings.Select(c => c.CampaignId).ToList();
        var results = _db.EmailEvents.Where(e => campaignIds.Contains(e.CampaignId)).ToList();
        var sentLogs = _db.SentLogs.Where(s => s.CreatedBy == userId && campaignIds.Contains(s.JobId)).ToList();
        if (start.HasValue)
        {
            results = results.Where(e => e.ReceivedOn >= start.Value).ToList();
            if (end.HasValue)
            {
                results = results.Where(e => e.ReceivedOn <= end.Value).ToList();
            }
        }
        StatisticsModel response = new()
        {
            SearchStartTime = start ?? DateTime.Now,
            SearchEndTime = end ?? DateTime.Now.AddDays(7),
            TotalEmailsSent = sentLogs.Count,
            TotalBounces = results.Count(e => e.Event == "bounce"),
            TotalDelivered = results.Count(e => e.Event == "delivered"),
            TotalDropped = results.Count(e => e.Event == "dropped"),
            TotalSpamReports = results.Count(e => e.Event == "spamreport"),
            TotalOpens = results.Count(e => e.Event == "open"),
            TotalClicks = results.Count(e => e.Event == "click"),
            TotalUnsubscribes = results.Count(e => e.Event == "unsubscribe"),
            TotalCampaigns = userCampaings.Count(e => e.IsEnabled && !e.Deleted)
        };
        return response;
    }
    //public ChartData GetLineChartDataAsync()
    //{

    //}
    public async Task<StatisticsModel> GetCampaignStatsAsync(int campaignId)
    {
        var campaign = await _db.Campaigns
            .Include(c => c.Audience!.SuppressionGroup!.Suppressions)
            .FirstOrDefaultAsync(c => c.CampaignId == campaignId);
        if (campaign is null)
            return new();

        var results = _db.EmailEvents.Where(e => campaignId == e.CampaignId).ToList();

        StatisticsModel response = new()
        {
            TotalEmailsSent = results.Count(e => e.Event == "processed"),
            TotalBounces = results.Count(e => e.Event == "bounce"),
            TotalDelivered = results.Count(e => e.Event == "delivered"),
            TotalDropped = results.Count(e => e.Event == "dropped"),
            TotalSpamReports = results.Count(e => e.Event == "spamreport"),
            TotalUniqueOpens = results.Where(e => e.Event == "open").Select(e => e.Email).Distinct().Count(),
            TotalOpens = results.Count(e => e.Event == "open"),
            TotalUniqueClicks = results.Where(e => e.Event == "click").Select(e => e.Email).Distinct().Count(),
            TotalClicks = results.Count(e => e.Event == "click"),
            TotalUnsubscribes = campaign.Audience?.SuppressionGroup?.Suppressions.Count??0,
        };
        return response;

    }
}