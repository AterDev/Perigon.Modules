namespace NotificationMod.Models.NotificationRuleDtos;

public class NotificationRuleItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public Guid[] ReceiverIds { get; set; } = [];
    public string[] EventTypes { get; set; } = [];
    public string[] Statuses { get; set; } = [];
    public long[] ProjectIds { get; set; } = [];
    public bool Enabled { get; set; }
}

