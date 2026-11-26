using EntityFramework.AppDbContext;

namespace AuditMod.Demands;

public interface IAuditNotificationDemand
{
    Task PublishAsync(
        DefaultDbContext context,
        AuditNotificationMessage message,
        CancellationToken cancellationToken = default);
}
