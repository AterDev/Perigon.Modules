namespace NotificationMod.Models.InAppNotificationDtos;

public class InAppNotificationDetailDto : InAppNotificationItemDto
{
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid[] TargetUserIds { get; set; } = [];
}
