namespace AuditMod.Models.AuditFlowDtos;
public class AuditFlowItemDto
{
    public Guid Id { get; set; }
    public string FlowKey { get; set; } = string.Empty;
    public string SceneCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
}
