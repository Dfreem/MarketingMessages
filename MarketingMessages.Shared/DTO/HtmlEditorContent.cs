using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO;

public class HtmlEditorContent
{
    public string DocumentTitle { get; set; } = "Untitled";
    public int SenderId { get; set; }
    public string Subject { get; set; } = "";
    public string HtmlContent { get; set; } = "";
    public string TextContent { get; set; } = "";
    public int ContentId { get; set; }
    public bool ShouldRefresh { get; set; }
    public int ListId { get; set; }
    public List<string> TemplateVariableNames { get; set; } = [];
    public Dictionary<string, string> Images { get; set; } = [];
    public Dictionary<string, string> ImageUrls { get; set; } = [];

}
