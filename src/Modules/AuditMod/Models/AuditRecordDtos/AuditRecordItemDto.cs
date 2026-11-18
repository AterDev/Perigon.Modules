namespace AuditMod.Models.AuditRecordDtos;
public class AuditRecordItemDto
{
    public Guid Id { get; set; }
    public string FlowNameSnapshot { get; set; } = string.Empty;
    public string SceneCode { get; set; } = string.Empty;
    public string BizId { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantNameSnapshot { get; set; } = string.Empty;
    public AuditRecordStatus Status { get; set; }
    public int? CurrentStepNo { get; set; }
    public DateTimeOffset SubmittedTime { get; set; }
    public DateTimeOffset? CompletedTime { get; set; }
    public DateTimeOffset? WithdrawnTime { get; set; }
}
