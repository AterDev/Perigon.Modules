namespace NotificationMod.Models.InAppNotificationDtos;

public class InAppNotificationItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedTime { get; set; }
    public DateTimeOffset? ExpiresTime { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
