using Perigon.AspNetCore.Attributes;

namespace SystemMod.Services;

public static class SystemLogBatchWriter
{
    public static async Task WriteAsync(
        DefaultDbContext context,
        IReadOnlyList<SysLogs> logs,
        CancellationToken cancellationToken)
    {
        foreach (SysLogs log in logs)
        {
            object? entity = log.Data;
            if (entity is string targetName)
            {
                log.TargetName = targetName;
            }
            else if (entity is not null)
            {
                LogDescriptionAttribute? attribute = entity
                    .GetType()
                    .GetCustomAttributes(typeof(LogDescriptionAttribute), inherit: false)
                    .OfType<LogDescriptionAttribute>()
                    .FirstOrDefault();
                if (attribute is not null)
                {
                    log.TargetName = attribute.Description;
                    log.Description ??= log.ActionType + attribute.Description;
                    if (entity is ILogDescriptionTarget target
                        && !string.IsNullOrWhiteSpace(target.LogTargetName))
                    {
                        log.TargetName = target.LogTargetName;
                    }
                }
            }
        }

        await context.SysLogs.BulkInsertAsync(logs, cancellationToken: cancellationToken);
    }
}
