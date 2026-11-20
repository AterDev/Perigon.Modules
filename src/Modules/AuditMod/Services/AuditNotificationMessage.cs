namespace AuditMod;

public sealed record AuditNotificationMessage(
    string Type,
    string Title,
    string Content,
    string Link,
    Guid CreatedBy,
    IReadOnlyCollection<Guid> TargetUserIds);
