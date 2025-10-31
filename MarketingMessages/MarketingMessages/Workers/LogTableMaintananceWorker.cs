using MarketingMessages.Data;

namespace MarketingMessages.Workers;

public class LogTableMaintananceWorker : BackgroundService
{
    IDbContextFactory<MarketingMessagesContext> _dbFactory;
    System.Timers.Timer? _timer;
    ILogger<LogTableMaintananceWorker> _logger;
    int _interval = 10;

    public LogTableMaintananceWorker(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        _dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MarketingMessagesContext>>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<LogTableMaintananceWorker>>();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var db = _dbFactory.CreateDbContext();
        var intervalSetting = db.Settings.FirstOrDefault(s => s.SettingName.StartsWith(SettingNameKeys.LogRolloverInterval));
        if(intervalSetting is not null)
            _interval = Convert.ToInt32(intervalSetting.SettingValue);
        await CleanLogsAsync();
        _timer = new();
        _timer.Interval = TimeSpan.FromDays(_interval).TotalMilliseconds;
        _timer.Enabled = true;
        _timer.AutoReset = true;
        _timer.Elapsed += async (_, args) =>
        {
            await CleanLogsAsync();
        };
        _timer.Start();
        await Task.CompletedTask;
    }

    private async Task CleanLogsAsync()
    {
        using var timerDb = _dbFactory.CreateDbContext();
        var deletes = timerDb.ApplicationLogs.Where(l => l.TimeStamp == null || l.TimeStamp.Value.AddDays(_interval) <= DateTime.Today);
        int totalDeleting = await deletes.CountAsync();
        _logger.LogInformation("LOG_ROLLOVER Deleting {totalDeleting} logs", totalDeleting);
        timerDb.ApplicationLogs.RemoveRange(deletes);
        timerDb.SaveChanges();
        if (timerDb.ApplicationLogs.Any(l => l.TimeStamp == null || l.TimeStamp.Value.AddDays(_interval) <= DateTime.Today))
            _logger.LogError("LOG_ROLLOVER Unable to delete application logs.");
    }
}
public static class SettingNameKeys
{
    public const string LogRolloverInterval = "Log Rollover Interval";
}
