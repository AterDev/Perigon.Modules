namespace Entity.NotificationMod;

[Index(nameof(EventId), nameof(ReceiverId), IsUnique = true)]
public class NotificationDelivery : EntityBase
{
    public Guid EventId { get; set; }
    public Guid ReceiverId { get; set; }
    [Required, StringLength(100)] public string ReceiverName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Status { get; set; } = "pending";
    [StringLength(2000)]
    public string? Error { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}
