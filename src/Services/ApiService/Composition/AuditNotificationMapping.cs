using AuditMod;
using NotificationMod.Models.InAppNotificationDtos;

namespace ApiService.Composition;

internal static class AuditNotificationMapping
{
    public static InAppNotificationPublishDto ToNotificationDto(this AuditNotificationMessage message)
    {
        return new InAppNotificationPublishDto(
            message.Type,
            message.Title,
            message.Content,
            message.Link,
            message.CreatedBy,
            message.TargetUserIds.ToArray());
    }
}
