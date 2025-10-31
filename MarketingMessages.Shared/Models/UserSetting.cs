using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Models;

public class Setting
{
    public int Id { get; set; }

    public required string Name { get; set; }
}

public class UserSetting
{
    public int Id { get; set; }
    public Setting Setting { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
}
