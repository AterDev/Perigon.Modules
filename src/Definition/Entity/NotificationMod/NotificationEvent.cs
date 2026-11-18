namespace Entity.NotificationMod;

[Index(nameof(SourceId), nameof(EventKey), IsUnique = true)]
public class NotificationEvent : EntityBase
{
    public Guid SourceId { get; set; }
    [Required, StringLength(128)] public string EventKey { get; set; } = string.Empty;
    [Required, StringLength(50)] public string EventType { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    [StringLength(100)] public string ObjectId { get; set; } = string.Empty;
    [StringLength(300)] public string ProjectName { get; set; } = string.Empty;
    [StringLength(500)] public string Namespace { get; set; } = string.Empty;
    [StringLength(300)] public string ActorName { get; set; } = string.Empty;
    [StringLength(500)] public string Title { get; set; } = string.Empty;
    [StringLength(8000)] public string Content { get; set; } = string.Empty;
    [StringLength(40)] public string Status { get; set; } = string.Empty;
    [StringLength(50)] public string Action { get; set; } = string.Empty;
    [StringLength(300)] public string Ref { get; set; } = string.Empty;
    public bool Tag { get; set; }
    [StringLength(64)] public string Sha { get; set; } = string.Empty;
    public DateTimeOffset? OccurredAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public double? Duration { get; set; }
    [StringLength(1200)] public string Url { get; set; } = string.Empty;
    [Required, StringLength(20)] public string ProcessingStatus { get; set; } = "processing";
}
