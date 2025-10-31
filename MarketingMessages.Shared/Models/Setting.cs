using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Models;

public class ApplicationSetting
{
    public int Id { get; set; }
    public required string SettingName { get; set; }
    public required string SettingValue { get; set; }
}
