namespace NotificationMod.Models.NotificationSourceDtos;

public class NotificationSourceItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool HasSecret { get; set; }
    public string AuthType { get; set; } = string.Empty;
    public string HeaderKey { get; set; } = string.Empty;
}

