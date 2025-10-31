using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO.V1;

public class EmailSender
{
    public int SenderId { get; set; }
    public string Name { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    /// <summary>
    /// If no Reply email is supplied, the FromEmail is used as both from and reply.
    /// </summary>
    public string? ReplyEmail { get; set; }

}
