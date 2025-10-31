using MarketingMessages.Shared.Enums;
using MarketingMessages.Shared.Models;

namespace MarketingMessages.Shared.DTO;

public class WizardRequest
{
    public int AudienceId { get; set; }
    public string DocumentTitle { get; set; } = "Untitled";
    public int ContentId { get; set; }
    public int SenderId  { get; set; }
    public string Subject { get; set; } = "";
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime EndTime { get; set; } = DateTime.Now;
    public bool IsEnabled { get; set; }
    public bool SendNow { get; set; }
    public HtmlEditorContent EditorContent { get; set; } = new();
    public bool Recurring { get; set; }
    public IntervalUnit FrequencyUnit { get; set; } 
    public int FrequencyInterval { get; set; }
    public int JobId { get; set; }
    public bool IsSending { get; set; }
    public bool IsComplete { get; set; }
    public bool CanDelete { get; set; }
    public int TotalSent { get; set; }
    public int TotalToSend { get; set; }
    public List<SentLog> Logs { get; set; } = [];
    public EmailJobType JobType { get; set; }

    public List<string> SubNames { get; set; } = [];
    public List<int> GroupIds { get; set; } = [];

    public Campaign AsCampaign(string userId)
    {
        throw new NotImplementedException();
    }
}

