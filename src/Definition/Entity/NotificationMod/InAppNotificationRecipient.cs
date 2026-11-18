namespace Entity.NotificationMod;

[Index(nameof(UserId), nameof(IsRead), nameof(CreatedTime))]
[Index(nameof(NotificationId), nameof(UserId), IsUnique = true)]
public class InAppNotificationRecipient : EntityBase
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadTime { get; set; }
}
