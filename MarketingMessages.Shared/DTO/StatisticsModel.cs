using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO
{
    public class StatisticsModel
    {
        public DateTime SearchStartTime { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime SearchEndTime { get; set; } = DateTime.Today;
        public int TotalEmailsSent { get; set; }
        public int TotalBounces { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalDropped { get; set; }
        public int TotalSpamReports { get; set; }
        public int TotalUniqueOpens { get; set; }
        public int TotalOpens { get; set; }
        public int TotalUniqueClicks { get; set; }
        public int TotalClicks { get; set; }
        public int TotalUnsubscribes { get; set; }
        public int TotalCampaigns { get; set; }
    }
}
