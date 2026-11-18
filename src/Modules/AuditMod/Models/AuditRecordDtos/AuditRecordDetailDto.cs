namespace AuditMod.Models.AuditRecordDtos;
public sealed class AuditRecordDetailDto : AuditRecordItemDto
{
    public string ApplyReason { get; set; } = string.Empty;
    public string? FormSnapshotJson { get; set; }
    public string? WithdrawReason { get; set; }
    public List<AuditRecordStepDto> Steps { get; set; } = [];
}
