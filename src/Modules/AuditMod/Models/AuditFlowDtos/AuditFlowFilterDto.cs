namespace AuditMod.Models.AuditFlowDtos;
public sealed class AuditFlowFilterDto : FilterBase
{
    public string? SceneCode { get; set; }
    public string? Keyword { get; set; }
    public bool? IsEnabled { get; set; }
}
