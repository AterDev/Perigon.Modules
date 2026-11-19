using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.InAppNotificationDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class InAppNotificationUpdateDto
{
    [StringLength(40)]
    public string? Type
    {
        get; set;
    }
    [StringLength(200)]
    public string? Title
    {
        get; set;
    }
    [StringLength(20000)]
    public string? Content
    {
        get; set;
    }
    [StringLength(1200)]
    public string? Link
    {
        get; set;
    }
    public DateTimeOffset? ExpiresTime
    {
        get; set;
    }
    public bool ClearExpiresTime
    {
        get; set;
    }
    public Guid[]? TargetUserIds
    {
        get; set;
    }
}
