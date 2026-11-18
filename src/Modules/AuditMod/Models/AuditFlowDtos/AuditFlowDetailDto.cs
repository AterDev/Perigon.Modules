namespace AuditMod.Models.AuditFlowDtos;
public sealed class AuditFlowDetailDto : AuditFlowItemDto { public List<AuditFlowStepDto> Steps { get; set; } = []; }
