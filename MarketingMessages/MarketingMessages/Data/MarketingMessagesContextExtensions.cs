using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Serilog;

namespace MarketingMessages.Data;

public static class MarketingMessagesContextExtensions
{
    public static List<Contact> ExecuteQueryForList(this MarketingMessagesContext db, int sendListId)
    {
        var sendList = db.Audiences.FirstOrDefault(a => a.Id == sendListId);
        if (sendList is null)
        {
            Log.Warning("Unable to find send list with id {sendListId}", sendListId);
            return [];
        }
        try
        {
            var contacts = db.Contacts.FromSqlRaw(sendList.Query).ToList();
            return contacts;
        }
        catch (Exception ex)
        {
            Log.Error("Query for Audience is invalid.\n{ex}", ex);
            return [];
        }

    }
    public static async Task<StatisticsModel> GetCampaignStatsAsync(this MarketingMessagesContext db, int campaignId)
    {
        var campaign = await db.Campaigns.FindAsync(campaignId);
        if (campaign is null)
            return new();

        var results = db.EmailEvents.Where(e => campaignId == e.CampaignId).ToList();

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
            TotalUnsubscribes = results.Count(e => e.Event == "unsubscribe")
        };
        return response;

    }


}
