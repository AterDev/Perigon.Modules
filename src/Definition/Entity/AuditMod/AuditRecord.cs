namespace Entity.AuditMod;

[Index(
    nameof(ApplicantUserId),
    nameof(SceneCode),
    nameof(BizId),
    IsUnique = true,
    Filter = "\"status_value\" = 0")]
public sealed class AuditRecord : EntityBase
{
    public Guid FlowId { get; set; }
    [StringLength(200), Required] public string FlowNameSnapshot { get; set; } = string.Empty;
    [StringLength(100), Required] public string SceneCode { get; set; } = string.Empty;
    [StringLength(200), Required] public string BizId { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    [StringLength(100), Required] public string ApplicantNameSnapshot { get; set; } = string.Empty;
    [StringLength(2000), Required] public string ApplyReason { get; set; } = string.Empty;
    public string? FormSnapshotJson { get; set; }
    public int StatusValue { get; set; } = (int)AuditRecordStatus.InProgress;
    [NotMapped]
    public AuditRecordStatus Status
    {
        get => (AuditRecordStatus)StatusValue; set => StatusValue = (int)value;
    }
    public int? CurrentStepNo { get; set; }
    public DateTimeOffset SubmittedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedTime { get; set; }
    public DateTimeOffset? WithdrawnTime { get; set; }
    [StringLength(2000)]
    public string? WithdrawReason { get; set; }
}

