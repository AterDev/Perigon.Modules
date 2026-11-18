namespace AuditMod.Models.AuditRecordDtos;
public sealed class AuditRecordFilterDto : FilterBase
{
    public AuditRecordStatus? Status { get; set; }
    public string? SceneCode { get; set; }
    public string? BizId { get; set; }
}
