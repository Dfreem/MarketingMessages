using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO.V1;

public class CustomValidation
{
    public string For { get; set; } = "";
    public bool? Valid { get; set; }
    public string Message { get; set; } = "";

}

