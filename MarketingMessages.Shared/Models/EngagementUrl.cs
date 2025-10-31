using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Models;

public class EngagementUrl
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public required string UserId { get; set; }
    [ForeignKey("Campaign")]
    public int CampaignId { get; set; }
    public required string Url { get; set; }
}
