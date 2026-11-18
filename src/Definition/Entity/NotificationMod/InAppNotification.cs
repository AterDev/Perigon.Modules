namespace Entity.NotificationMod;

/// <summary>Business generated in-app message or announcement.</summary>
public class InAppNotification : EntityBase
{
    [Required, StringLength(40)] public string Type { get; set; } = "message";
    [Required, StringLength(200)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(20000)] public string Content { get; set; } = string.Empty;
    [StringLength(1200)] public string Link { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedTime { get; set; }
    public DateTimeOffset? ExpiresTime { get; set; }
    public Guid? CreatedBy { get; set; }
    /// <summary>Empty means all users; otherwise only listed users receive a row.</summary>
    public Guid[] TargetUserIds { get; set; } = [];
}
