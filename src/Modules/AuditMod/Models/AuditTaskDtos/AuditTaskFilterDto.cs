namespace AuditMod.Models.AuditTaskDtos;
public sealed class AuditTaskFilterDto : FilterBase
{
    public AuditTaskStatus? Status { get; set; }
    public AuditRecordStatus? RecordStatus { get; set; }
}
