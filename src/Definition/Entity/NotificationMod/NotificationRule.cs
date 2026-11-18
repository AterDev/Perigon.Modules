namespace Entity.NotificationMod;

/// <summary>Maps normalized event types from a source to one or more receivers.</summary>
public class NotificationRule : EntityBase
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public bool Enabled { get; set; } = true;
    public string[] EventTypes { get; set; } = ["pipeline", "push", "tag_push", "merge_request"];
    public string[] Statuses { get; set; } = ["success", "failed", "canceled"];
    public long[] ProjectIds { get; set; } = [];
}
