using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO;

public class SegmentModel
{
    public string Name { get; set; } = "Untitled";
    public List<ContactModel> Contacts { get; set; } = [];
}
