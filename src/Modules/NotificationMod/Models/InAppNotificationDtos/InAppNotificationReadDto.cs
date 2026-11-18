namespace NotificationMod.Models.InAppNotificationDtos;

public class InAppNotificationReadDto
{
    public Guid Id { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadTime { get; set; }
}
