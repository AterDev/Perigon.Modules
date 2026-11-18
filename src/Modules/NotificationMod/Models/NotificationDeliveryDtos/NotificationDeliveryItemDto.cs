namespace NotificationMod.Models.NotificationDeliveryDtos;

public sealed class NotificationDeliveryItemDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ObjectId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}
