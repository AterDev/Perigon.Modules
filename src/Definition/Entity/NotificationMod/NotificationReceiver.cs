namespace Entity.NotificationMod;

/// <summary>Outbound notification endpoint and its delivery policy.</summary>
public class NotificationReceiver : EntityBase
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(40)] public string Type { get; set; } = "webhook";
    public bool Enabled { get; set; } = true;
    [Required, StringLength(2048)] public string ProtectedSecret { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string Endpoint { get; set; } = string.Empty;
    [Required, StringLength(10)] public string HttpMethod { get; set; } = "POST";
    [Required, StringLength(20)] public string AuthType { get; set; } = "query";
    [StringLength(100)] public string AuthName { get; set; } = "key";
    [StringLength(40)] public string AuthScheme { get; set; } = string.Empty;
    [Required, StringLength(8000)] public string BodyTemplate { get; set; } = "{\"content\":\"{content}\"}";
    public int TimeoutSeconds { get; set; } = 3;
    public int RateLimitPerMinute { get; set; } = 20;
    [StringLength(100)] public string SuccessField { get; set; } = string.Empty;
    [StringLength(200)] public string SuccessValue { get; set; } = string.Empty;
}
