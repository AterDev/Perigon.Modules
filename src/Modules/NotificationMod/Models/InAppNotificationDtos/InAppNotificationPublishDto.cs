namespace NotificationMod.Models.InAppNotificationDtos;

public sealed record InAppNotificationPublishDto(
    string Type,
    string Title,
    string Content,
    string Link,
    Guid SenderId,
    Guid[] RecipientIds);
