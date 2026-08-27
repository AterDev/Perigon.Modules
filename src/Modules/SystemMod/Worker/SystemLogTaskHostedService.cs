using System.Reflection;
using EntityFramework.AppDbFactory;
using Perigon.AspNetCore.Attributes;
using Microsoft.Extensions.Hosting;
using Share.Services;

namespace SystemMod.Worker;

/// <summary>
/// 日志记录任务
/// </summary>
public class SystemLogTaskHostedService(
    IServiceProvider serviceProvider,
    IEntityTaskQueue<SystemLogs> queue,
    ILogger<SystemLogTaskHostedService> logger
) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<SystemLogTaskHostedService> _logger = logger;
    private readonly IEntityTaskQueue<SystemLogs> _taskQueue = queue;

    private readonly int MaxWaitMilliseconds = 10_000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"🚀 System Log Hosted Service is running.");
        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        List<SystemLogs> logs = [];
        DateTime? batchStart = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            var log = await _taskQueue.DequeueAsync(stoppingToken);
            batchStart ??= DateTime.Now;
            logs.Add(log);
            if (
                logs.Count >= 10
                || (DateTime.Now - batchStart.Value).TotalMilliseconds > MaxWaitMilliseconds
            )
            {
                await InsertLogsAsync(logs, stoppingToken);
                logs.Clear();
                batchStart = null;
            }
        }

        // 插入剩余的日志
        if (logs.Count > 0)
        {
            await InsertLogsAsync(logs, stoppingToken);
        }
    }

    private async Task InsertLogsAsync(List<SystemLogs> logs, CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<AppDbFactory>();
        var tenantService = scope.ServiceProvider.GetRequiredService<TenantService>();

        foreach (var log in logs)
        {
            var entity = log.Data;
            if (entity is string)
            {
                log.TargetName = entity as string;
            }
            else if (entity != null)
            {
                var type = entity.GetType();
                var attribute = type?.GetCustomAttribute<LogDescriptionAttribute>();
                if (attribute != null)
                {
                    log.TargetName = attribute.Description;
                    log.Description ??= log.ActionType + attribute.Description;
                    if (attribute.FieldName != null)
                    {
                        var fieldValue = type!.GetProperty(attribute.FieldName)?.GetValue(entity);
                        if (fieldValue != null)
                        {
                            log.TargetName = fieldValue as string;
                        }
                    }
                }
            }
        }

        foreach (var tenantLogs in logs.GroupBy(log => log.TenantId))
        {
            if (tenantLogs.Key == Guid.Empty)
            {
                _logger.LogWarning("Skip {Count} system logs without a TenantId", tenantLogs.Count());
                continue;
            }

            try
            {
                var tenant = await tenantService.GetByIdAsync(tenantLogs.Key, stoppingToken);
                if (tenant is null)
                {
                    _logger.LogWarning("Skip logs for unknown tenant {TenantId}", tenantLogs.Key);
                    continue;
                }

                await using var context = dbContextFactory.CreateDbContext(tenantLogs.Key);
                var tenantLogList = tenantLogs.ToList();
                context.AddRange(tenantLogList);
                await context.SaveChangesAsync(stoppingToken);
                foreach (var log in tenantLogList)
                {
                    _logger.LogInformation(
                        "✍️ New Log:[{object}] {actionUser} {action} {name}",
                        log.Description,
                        log.ActionUserName,
                        log.ActionType,
                        log.TargetName
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred executing batch logs for tenant {TenantId}. Count: {count}",
                    tenantLogs.Key,
                    tenantLogs.Count()
                );
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🛑 System Log Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
