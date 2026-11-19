namespace Entity.AuditMod;

[Index(nameof(AuditRecordId), nameof(StepNoSnapshot), IsUnique = true)]
public sealed class AuditRecordDetail : EntityBase
{
    public Guid AuditRecordId { get; set; }
    public Guid StepDefinitionId { get; set; }
    public int StepNoSnapshot { get; set; }
    [StringLength(200), Required] public string StepNameSnapshot { get; set; } = string.Empty;
    [Column("AuditModeSnapshotValue")]
    public AuditMode AuditModeSnapshot { get; set; }
    [Column("StatusValue")]
    public AuditStepStatus Status { get; set; } = AuditStepStatus.Waiting;
    public DateTimeOffset? StartedTime { get; set; }
    public DateTimeOffset? CompletedTime { get; set; }
}

