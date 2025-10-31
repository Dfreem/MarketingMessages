using MarketingMessages.Shared.Enums;
using MarketingMessages.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO.V1;

public class EmailSchedule
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Recurring { get; set; }
    public IntervalUnit FrequencyUnit { get; set; }
    public int FrequencyInterval { get; set; }
    public string TextContent { get; set; } = "";
    public string HtmlContent { get; set; } = "";
    public int SenderId { get; set; }
    public int JobId { get; set; }
    public bool IsSending { get; set; }
    public bool IsComplete { get; set; }
    public bool CanDelete { get; set; }
    public int TotalSent { get; set; }
    public int TotalToSend { get; set; }
    public int ContentId { get; set; }
    public List<SentLog> Logs { get; set; } = [];
    public int ListId { get; set; }
    public string EmailSubject { get; set; } = "";
    public EmailJobType JobType { get; set; }

    public List<string> SubNames { get; set; } = [];
    public bool SendNow { get; set; }

    public EmailSchedule()
    {

    }
    public EmailSchedule(HtmlEditorContent content)
    {
        HtmlContent = content.HtmlContent;
        TextContent = content.TextContent;
        SubNames = content.TemplateVariableNames;

    }
}

