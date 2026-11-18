namespace AuditMod.Models.AuditTaskDtos;
public sealed class AuditTaskDetailDto : AuditTaskItemDto { public AuditRecordDetailDto Record { get; set; } = new(); }
