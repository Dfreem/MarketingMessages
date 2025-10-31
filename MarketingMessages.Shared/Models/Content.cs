using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketingMessages.Shared.Models;

public partial class Content
{
    public int ContentId { get; set; }
    public string Name { get; set; } = "";
    public string TextContent { get; set; } = default!;
    public string HtmlContent { get; set; } = default!;
    public required string CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Comma seperated string of properties to replace. All of these properties come from the <see cref="Contact"/> class
    /// </summary>
    public string TemplatePropertyNames { get; set; } = "";
    public List<EmailContentImages> Images { get; set; } = [];
    public bool Deleted { get; set; }

    public string ReplaceSubstitutionTags(Contact contact)
    {
        var replacements = TemplatePropertyNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries);
        string result = HtmlContent.Clone().ToString()??"";
        foreach (var replacement in replacements)
        {
            var propertyName = replacement.Replace("#", "");
            var propertyValue = typeof(Contact).GetProperty(propertyName)?.GetValue(contact);
            if (propertyValue is null)
                continue;
            result = result.Replace(replacement, propertyValue.ToString());
        }
        return result;

    }
}