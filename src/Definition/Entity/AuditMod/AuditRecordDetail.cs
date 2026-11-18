namespace Entity.AuditMod;

[Index(nameof(AuditRecordId), nameof(StepNoSnapshot), IsUnique = true)]
public sealed class AuditRecordDetail : EntityBase
{
    public Guid AuditRecordId { get; set; }
    public Guid StepDefinitionId { get; set; }
    public int StepNoSnapshot { get; set; }
    [StringLength(200), Required] public string StepNameSnapshot { get; set; } = string.Empty;
    public int AuditModeSnapshotValue { get; set; }
    [NotMapped]
    public AuditMode AuditModeSnapshot
    {
        get => (AuditMode)AuditModeSnapshotValue; set => AuditModeSnapshotValue = (int)value;
    }
    public int StatusValue { get; set; } = (int)AuditStepStatus.Waiting;
    [NotMapped]
    public AuditStepStatus Status
    {
        get => (AuditStepStatus)StatusValue; set => StatusValue = (int)value;
    }
    public DateTimeOffset? StartedTime { get; set; }
    public DateTimeOffset? CompletedTime { get; set; }
}

