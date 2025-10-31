using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

using MarketingMessages.Shared.Enums;
using System.Text.Json.Serialization;

namespace MarketingMessages.Shared.Models;

public partial class Campaign
{

    public int CampaignId { get; set; }
    public int SenderId { get; set; }
    public Sender Sender { get; set; } = default!;
    public string? CampaignType { get; set; }
    public string FrequencyUnit { get; set; } = IntervalUnit.Month.ToString();
    public int FrequencyInterval { get; set; }
    public DateTime? StartDate { get; set; }
    [AllowNull]
    public DateTime? EndDate { get; set; }
    public bool IsEnabled { get; set; }

    public bool IsStarted { get; set; }

    public bool IsExecuting { get; set; }

    public bool IsComplete { get; set; }
    public int JobsCompleted { get; set; }
    public int TotalJobLimit { get; set; }

    public bool IsRecurring { get; set; }
    public DateTime? NextExecution { get; set; }

    /// <summary>
    /// The id of the user who was logged in when this job was created.
    /// </summary>
    public required string CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// If the job has been modified, this is the id of the user that was logged in when it was modified.
    /// </summary>
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }
    public int EmailContentId { get; set; }
    public Content EmailContent { get; set; } = default!;

    [ForeignKey(nameof(Audience))]
    public int AudienceId { get; set; }
    public Audience? Audience { get; set; }
    public string Subject { get; set; } = "";
    public bool Deleted { get; set; }
    public string Name { get; set; } = "Untitled";

    public Campaign Clone()
    {
        return (Campaign)MemberwiseClone();
    }

}