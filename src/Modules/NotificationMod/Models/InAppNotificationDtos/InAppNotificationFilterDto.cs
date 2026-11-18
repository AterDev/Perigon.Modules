using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.InAppNotificationDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class InAppNotificationFilterDto : FilterBase
{
    [StringLength(40)] public string? Type { get; set; }
    [StringLength(200)] public string? Title { get; set; }
    public bool? Published { get; set; }
}
