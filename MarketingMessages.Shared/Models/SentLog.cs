using System;
using System.Collections.Generic;

namespace MarketingMessages.Shared.Models;

public partial class SentLog
{
    public int SentLogId { get; set; }

    public int JobId { get; set; }

    public int EmailContentId { get; set; }
    public int SendListId { get; set; }
    public string? Subject { get; set; }

    public string? Body { get; set; }

    public bool Success { get; set; }

    public string? Result { get; set; }

    public DateTime DateSent { get; set; }

    public required string CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }
}