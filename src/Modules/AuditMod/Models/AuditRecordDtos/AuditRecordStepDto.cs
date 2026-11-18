namespace AuditMod.Models.AuditRecordDtos;
public sealed class AuditRecordStepDto
{
    public Guid Id { get; set; }
    public int StepNoSnapshot { get; set; }
    public string StepNameSnapshot { get; set; } = string.Empty;
    public AuditMode AuditModeSnapshot { get; set; }
    public AuditStepStatus Status { get; set; }
    public DateTimeOffset? StartedTime { get; set; }
    public DateTimeOffset? CompletedTime { get; set; }
    public List<AuditRecordTaskDto> Tasks { get; set; } = [];
}
