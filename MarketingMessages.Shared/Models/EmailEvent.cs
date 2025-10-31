using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Models;

public class EmailEvent
{
    public int Id { get; set; }
    [JsonPropertyName("event")]
    public string Event { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";
    [JsonPropertyName("category")]
    public string? Category { get; set; }
    [JsonPropertyName("smtp-id")]
    public string? SmtpId { get; set; }
    [JsonPropertyName("sg_event_id")]
    public string? SgEventId { get; set; }
    [JsonPropertyName("sg_message_id")]
    public string? SgMessageId { get; set; }
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }   // UNIX timestamp
    [JsonPropertyName("useragent")]
    public string? UserAgent { get; set; }
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("response")]
    public string? Response { get; set; }
    [JsonPropertyName("contact_id")]
    public int ContactId { get; set; }
    [JsonPropertyName("campaign_id")]
    public int CampaignId { get; set; }
    public DateTime ReceivedOn { get; set; }
}
