using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Components.Tabs;

public interface ITab
{
    string Name { get; set; }
    Task Show();
    Task Hide();
}
