namespace AuditMod.Models.AuditRecordDtos;
public sealed class AuditRecordTaskDto
{
    public Guid Id { get; set; }
    public Guid AuditRecordDetailId { get; set; }
    public Guid AssigneeUserId { get; set; }
    public string AssigneeNameSnapshot { get; set; } = string.Empty;
    public AuditTaskStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset? ActedTime { get; set; }
}
