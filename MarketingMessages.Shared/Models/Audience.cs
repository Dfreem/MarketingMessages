using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MarketingMessages.Shared.DTO;

namespace MarketingMessages.Shared.Models;

[Index(nameof(Name), nameof(CreatedBy), IsUnique = true)]
public class Audience
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public required string CreatedBy { get; set; }
    public required DateTime CreatedOn { get; set; } = DateTime.Now;
    public bool Deleted { get; set; }
    public required string Query { get; set; }
    public int? SuppressionGroupId { get; set; }
    [ForeignKey(nameof(SuppressionGroupId))]
    public SuppressionGroup? SuppressionGroup { get; set; }
    public string JSONForm { get; set; } = "";
}
