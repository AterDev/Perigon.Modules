using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.NotificationReceiverDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class NotificationReceiverAddDto
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(40)] public string Type { get; set; } = "webhook";
    public bool Enabled { get; set; } = true;
    [StringLength(256)] public string? Secret { get; set; }
    [Required, StringLength(2000)] public string Endpoint { get; set; } = string.Empty;
    [StringLength(10)] public string HttpMethod { get; set; } = "POST";
    [StringLength(20)] public string AuthType { get; set; } = "query";
    [StringLength(100)] public string AuthName { get; set; } = "key";
    [StringLength(40)] public string AuthScheme { get; set; } = string.Empty;
    [Required, StringLength(8000)] public string BodyTemplate { get; set; } = "{\"content\":\"{content}\"}";
    public int TimeoutSeconds { get; set; } = 3;
    public int RateLimitPerMinute { get; set; } = 20;
    [StringLength(100)] public string SuccessField { get; set; } = string.Empty;
    [StringLength(200)] public string SuccessValue { get; set; } = string.Empty;
}

