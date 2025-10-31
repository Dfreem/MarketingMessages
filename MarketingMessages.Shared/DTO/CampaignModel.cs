using MarketingMessages.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MarketingMessages.Shared.DTO;

public class CampaignModel
{
    public string Name { get; set; } = "";
    public int AudienceId { get; set; }
    public int CampaignId { get; set; }
    public Content Content { get; set; } = default!;
    public string? JobType { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsStarted { get; set; }
    public bool IsComplete { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextRunDate { get; set; }
    public DateTime? StartDate { get; set; }
    public string StatusBadge { get; set; } = "<div class='badge bg-300'>Not Started</div>";
    public DateTime CreatedOn { get; set; }
    public List<ContactModel> Contacts { get; set; } = [];
    public int Clicks { get; set; }
    public int Opens { get; set; }
    public int Sent { get; set; }
    public int Bounced { get; set; }
}

