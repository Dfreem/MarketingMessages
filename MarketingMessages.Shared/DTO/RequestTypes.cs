using MarketingMessages.Shared.Enums;
using MarketingMessages.Shared.Models;

using Microsoft.Extensions.Logging;

namespace MarketingMessages.Shared.DTO;

#region V1
public class EmailRequest
{
    public string TextContent { get; set; } = "";
    public string HtmlContent { get; set; } = "";
    public int? EmailContentId { get; set; }
    public string Subject { get; set; } = default!;
    public int SenderId { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> TemplateVariableNames { get; set; } = [];
    public List<ContentImage> Images { get; set; } = [];
    public string ContentName { get; set; } = "";

    public EmailRequest()
    {

    }
    public EmailRequest(HtmlEditorContent content)
    {
        TemplateVariableNames = content.TemplateVariableNames;
        TextContent = content.TextContent;
        HtmlContent = content.HtmlContent;
        ContentName = content.DocumentTitle;
        Subject = content.Subject;
        SenderId = content.SenderId;
        foreach (var item in content.Images.Keys)
        {
            var img = content.Images[item];
            if(img.Contains(','))
            {
                content.Images[item] = img.Split(',')[^1];
            }
        }
        Images = content.Images.Select(i => new ContentImage() { Name = i.Key, ImageData = Convert.FromBase64String(i.Value)}).ToList();
        EmailContentId = content.ContentId;
    }
}

public class SendListRequest
{
    public string? SearchText { get; set; }
    public string ListName { get; set; } = "";
    public List<string> Zipcodes { get; set; } = [];
    public List<string> Professions { get; set; } = [];
    public List<string> States { get; set; } = [];
    /// <summary>
    /// If CountAll is set, all search criteria are ignored and instead all of the current users available contacts are returned.
    /// </summary>
    public bool CountAll { get; set; }
}

public class ApplicationLogRequest
{
    public LoggingLevel Level { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class ContactsRequest
{
    public int Index { get; set; }
    public int PageSize { get; set; } = 100;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
}

public class TestEmailRequest
{
    public string Email { get; set; } = "";
    public int CampaignId { get; set; }
}


#endregion
