using EntityFramework.AppDbContext;

namespace AuditMod.Demands;

public delegate Task AuditNotificationPublisher(
    DefaultDbContext context,
    AuditNotificationMessage message,
    CancellationToken cancellationToken);
