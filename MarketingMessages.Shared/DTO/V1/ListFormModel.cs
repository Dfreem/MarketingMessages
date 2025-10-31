using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO.V1;

public class ListFormModel
{
    public List<string> ProfessionDropdown { get; set; } = [];
    public List<string> StatesDropdown { get; set; } = [];
    public Dictionary<string, bool> Zipcodes { get; set; } = [];
    public Dictionary<string, int> SendLists { get; set; } = [];

}
