using Microsoft.Extensions.Logging;

namespace MarketingMessages.Shared.Enums;

[Flags]
public enum LoggingLevel
{
    None = 0,
    Information = LogLevel.Information,
    Warning = LogLevel.Warning,
    Error = LogLevel.Error,
}