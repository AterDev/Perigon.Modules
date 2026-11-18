namespace NotificationMod.Models.NotificationReceiverDtos;

public class NotificationReceiverItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool HasSecret { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string AuthType { get; set; } = string.Empty;
    public string AuthName { get; set; } = string.Empty;
    public string AuthScheme { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; }
    public int RateLimitPerMinute { get; set; }
    public string SuccessField { get; set; } = string.Empty;
    public string SuccessValue { get; set; } = string.Empty;
}

