namespace Entity.NotificationMod;

/// <summary>Inbound notification source used by webhook integrations.</summary>
public class NotificationSource : EntityBase
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(40)] public string Type { get; set; } = "gitlab";
    public bool Enabled { get; set; } = true;
    [Required, StringLength(2048)] public string ProtectedSecret { get; set; } = string.Empty;
    [Required, StringLength(20)] public string AuthType { get; set; } = "query";
    [Required, StringLength(100)] public string HeaderKey { get; set; } = "X-Gitlab-Token";
}
