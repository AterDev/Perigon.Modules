namespace NotificationMod.Models.NotificationEventDtos;

public class NotificationEventItemDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset? OccurredAt { get; set; }
    public string ProcessingStatus { get; set; } = string.Empty;
}

