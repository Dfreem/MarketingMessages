using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Components.Tabs;

public interface ITabPane
{
    string Name { get; set; }
    Task Show();
    Task Hide();
}
