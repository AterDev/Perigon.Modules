using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.NotificationRecoveryDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public sealed class NotificationRecoveryDto
{
    [Range(60, 86400)] public int LeaseSeconds { get; set; } = 300;
}
