namespace NotificationMod.Models.WebhookDtos;

public sealed class NormalizedEventDto
{
    public string EventKey { get; set; } = string.Empty;
    public string EventType { get; set; } = "event";
    public long ProjectId { get; set; }
    public string ObjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Ref { get; set; } = string.Empty;
    public bool Tag { get; set; }
    public string Sha { get; set; } = string.Empty;
    public DateTimeOffset? OccurredAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public double? Duration { get; set; }
    public string Url { get; set; } = string.Empty;
}
