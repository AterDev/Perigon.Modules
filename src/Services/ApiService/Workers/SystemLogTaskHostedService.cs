using Entity.SystemMod;
using EntityFramework.AppDbContext;
using Perigon.PostgreSQL;
using SystemMod.Services;

namespace ApiService.Workers;

internal sealed class SystemLogTaskHostedService(
    IServiceProvider serviceProvider,
    IEntityTaskQueue<SysLogs> queue,
    ILogger<SystemLogTaskHostedService> logger
) : BackgroundService
{
    private const int MaxWaitMilliseconds = 10_000;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<SystemLogTaskHostedService> _logger = logger;
    private readonly IEntityTaskQueue<SysLogs> _taskQueue = queue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("System log hosted service is running.");
        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        List<SysLogs> logs = [];
        DateTime? batchStart = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            SysLogs log = await _taskQueue.DequeueAsync(stoppingToken);
            batchStart ??= DateTime.Now;
            logs.Add(log);
            if (logs.Count >= 10
                || (DateTime.Now - batchStart.Value).TotalMilliseconds > MaxWaitMilliseconds)
            {
                await InsertLogsAsync(logs, stoppingToken);
                logs.Clear();
                batchStart = null;
            }
        }

        if (logs.Count > 0)
        {
            await InsertLogsAsync(logs, stoppingToken);
        }
    }

    private async Task InsertLogsAsync(
        List<SysLogs> logs,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        DefaultDbContext context = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

        try
        {
            await SystemLogBatchWriter.WriteAsync(context, logs, cancellationToken);
            foreach (SysLogs log in logs)
            {
                _logger.LogInformation(
                    "New log: {Description} {ActionUser} {Action} {Name}",
                    log.Description,
                    log.ActionUserName,
                    log.ActionType,
                    log.TargetName);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "Error occurred executing log batch. Count: {Count}",
                logs.Count);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("System log hosted service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
