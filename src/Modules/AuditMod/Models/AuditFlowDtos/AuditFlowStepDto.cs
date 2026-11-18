namespace AuditMod.Models.AuditFlowDtos;
public sealed class AuditFlowStepDto
{
    public int StepNo { get; set; }
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;
    public List<Guid> AuditorUserIds { get; set; } = [];
    public AuditMode AuditMode { get; set; }
}
