namespace NotificationMod.Models.WebhookDtos;

public sealed class WebhookResultDto
{
    public Guid EventId { get; set; }
    public bool Duplicate { get; set; }
    public int DeliveryCount { get; set; }
    public int FailedCount { get; set; }
}
