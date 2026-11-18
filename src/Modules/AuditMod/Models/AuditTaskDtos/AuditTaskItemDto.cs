namespace AuditMod.Models.AuditTaskDtos;
public class AuditTaskItemDto
{
    public Guid Id { get; set; }
    public Guid AuditRecordId { get; set; }
    public Guid AuditRecordDetailId { get; set; }
    public Guid AssigneeUserId { get; set; }
    public string AssigneeNameSnapshot { get; set; } = string.Empty;
    public AuditTaskStatus Status { get; set; }
    public int StepNo { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string BizId { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTimeOffset? ActedTime { get; set; }
}
