using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.NotificationSourceDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class NotificationSourceAddDto
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(40)] public string Type { get; set; } = "gitlab";
    public bool Enabled { get; set; } = true;
    [StringLength(256)] public string? Secret { get; set; }
    [StringLength(20)] public string AuthType { get; set; } = "query";
    [StringLength(100)] public string HeaderKey { get; set; } = "X-Gitlab-Token";
}

