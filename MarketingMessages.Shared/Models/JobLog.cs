using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;

namespace MarketingMessages.Shared.Models;

public partial class JobLog
{
    public int JobLogId { get; set; }

    public int JobId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? FinishDate { get; set; }

    public bool? Succeeded { get; set; }

    public string? Notes { get; set; }
}