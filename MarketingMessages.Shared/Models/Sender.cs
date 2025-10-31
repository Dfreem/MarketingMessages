using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketingMessages.Shared.Models;

public class Sender
{
    public int SenderId { get; set; }
    public required string Name { get; set; } = "";
    public required string Email { get; set; } = "";
    public required string ReplyTo { get; set; } = "";
    public required string CreatedBy { get; set; }

    public ICollection<Campaign> Jobs { get; set; } = [];
}
