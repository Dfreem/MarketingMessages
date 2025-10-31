using MarketingMessages.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO
{
    public class AudienceSegment
    {
        public Audience Audience { get; set; } = default!;
        public SuppressionGroupModel? UnsubscribeGroup { get; set; }

        public int ContactCount { get; set; }
    }
}
