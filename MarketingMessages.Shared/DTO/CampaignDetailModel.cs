using MarketingMessages.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MarketingMessages.Shared.DTO;


public class CampaignDetailModel
{
    public string Subject { get; set; } = "";
    public string Name { get; set; } = "";
    public int AudienceId { get; set; }
    public int CampaignId { get; set; }
    public Content Content { get; set; } = default!;
    //public string? JobType { get; set; }
    public bool IsEnabled { get; set; }
    //public bool IsRecurring { get; set; }
    public bool IsStarted { get; set; }
    public bool IsComplete { get; set; }
    public string? From { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextRunDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public List<ContactModel> Contacts { get; set; } = [];
    public StatisticsModel? Stats { get; set; }
    public SuppressionGroupModel? UnsubscribeGroup { get; set; }
}

public static class CampaignModelExtensions
{
    public static Campaign ToCampaign(this CampaignDetailModel model, string userId)
    {
        return new()
        {
            CreatedBy = userId,
            CreatedOn = model.CreatedOn,
            AudienceId = model.AudienceId,
            CampaignId = model.CampaignId,
            EmailContent = model.Content,
            EmailContentId = model.Content.ContentId,
            EndDate = model.EndDate,
            StartDate = model.StartDate,
            //IsRecurring = model.IsRecurring,
            IsComplete = model.IsComplete,
            IsEnabled = model.IsEnabled,
            //JobType = model.JobType,
            Subject = model.Subject,
            Name = model.Name,
            //NextExecution = model.NextRunDate
        };
    }

    public static void UpdateWith(this Campaign campaign, CampaignDetailModel model)
    {
        campaign.AudienceId = model.AudienceId;
        campaign.CampaignId = model.CampaignId;
        campaign.EmailContent = model.Content;
        campaign.EmailContentId = model.Content.ContentId;
        campaign.EndDate = model.EndDate;
        campaign.StartDate = model.StartDate;
        //campaign.IsRecurring = model.IsRecurring;
        campaign.IsComplete = model.IsComplete;
        campaign.IsEnabled = model.IsEnabled;
        //campaign.JobType = model.JobType;
        campaign.Subject = model.Subject;
        campaign.NextExecution = model.NextRunDate;
    }

    public static CampaignDetailModel ToDetailsDisplay(this Campaign campaign)
    {
        return new()
        {
            Name = campaign.Name,
            CreatedOn = campaign.CreatedOn,
            From = campaign.Sender?.ReplyTo,
            IsEnabled = campaign.IsEnabled,
            Content = campaign.EmailContent,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            NextRunDate = campaign.NextExecution,
            IsComplete = campaign.IsComplete,
            CampaignId = campaign.CampaignId,
            AudienceId = campaign.AudienceId,
            IsStarted = campaign.IsStarted,
            //JobType = campaign.JobType,
            //IsRecurring = campaign.IsRecurring,
            Subject = campaign.Subject,
        };
    }
}
