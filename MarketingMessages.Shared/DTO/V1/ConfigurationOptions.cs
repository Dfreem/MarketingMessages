using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO.V1;

public class SendGridOptions
{
    public const string ConfigurationKey = "SendGridApi";
    public string ApiKey { get; set; } = default!;
    public string KeyId { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public bool SandboxMode { get; set; }
}

public class SmtpOptions
{
    public const string ConfigurationKey = "SendGridSMTP";
    public string Server { get; set; } = default!;
    public int Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;

}
