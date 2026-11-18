namespace Entity.NotificationMod;

public class InAppNotificationRecipient : EntityBase
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadTime { get; set; }
}
