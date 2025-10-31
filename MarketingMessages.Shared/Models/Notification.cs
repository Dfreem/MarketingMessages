using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Models;

public class Notification
{
    public int NotificationId { get; set; }

    [ForeignKey("User")]
    public required string UserId { get; set; }

    public bool Read { get; set; }

    public required string Message { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? TimeStamp { get; set; }
}
