using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Models
{
    public class SuppressionGroup
    {
        public int SuppressionGroupId { get; set; }
        public required string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime CreatedOn { get; set; }
        public required string CreatedBy { get; set; }
        public List<Contact> Suppressions { get; set; } = [];
        public bool Deleted { get; set; }
    }
}
