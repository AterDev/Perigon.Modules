namespace Entity.AuditMod;

[Index(nameof(AuditRecordDetailId), nameof(AssigneeUserId), IsUnique = true)]
public sealed class AuditTask : EntityBase
{
    public Guid AuditRecordDetailId { get; set; }
    public Guid AssigneeUserId { get; set; }
    [StringLength(100), Required] public string AssigneeNameSnapshot { get; set; } = string.Empty;
    public int StatusValue { get; set; } = (int)AuditTaskStatus.Waiting;
    [NotMapped]
    public AuditTaskStatus Status
    {
        get => (AuditTaskStatus)StatusValue; set => StatusValue = (int)value;
    }
    [StringLength(2000)]
    public string? Comment { get; set; }
    public DateTimeOffset? ActedTime { get; set; }
}
