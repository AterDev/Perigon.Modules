using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.InAppNotificationDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class InAppNotificationAddDto
{
    [Required, StringLength(40)] public string Type { get; set; } = "message";
    [Required, StringLength(200)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(20000)] public string Content { get; set; } = string.Empty;
    [StringLength(1200)] public string Link { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresTime { get; set; }
    public Guid[] TargetUserIds { get; set; } = [];
    public bool Publish { get; set; }
}
