using MarketingMessages.Shared.DTO;

using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Hubs;

public class NotificationHub : Hub<INotificationClient>
{
    public async Task SendTotalNotificationsAsync(string user, int totalNotifications) =>
        await Clients.User(user).TotalNotificationsChanged(totalNotifications);
    public async Task SendCampaignStatus(string user, CampaignDetailModel campaign) =>
        await Clients.User(user).CampaignStatusChanged(campaign);
}

public interface INotificationClient
{
    Task TotalNotificationsChanged(int totalUnread);

    Task CampaignStatusChanged(CampaignDetailModel model);

}
