namespace Entity.AuditMod;

[Index(nameof(AuditRecordDetailId), nameof(AssigneeUserId), IsUnique = true)]
public sealed class AuditTask : EntityBase
{
    public Guid AuditRecordDetailId { get; set; }
    public Guid AssigneeUserId { get; set; }
    [StringLength(100), Required] public string AssigneeNameSnapshot { get; set; } = string.Empty;
    [Column("StatusValue")]
    public AuditTaskStatus Status { get; set; } = AuditTaskStatus.Waiting;
    [StringLength(2000)]
    public string? Comment { get; set; }
    public DateTimeOffset? ActedTime { get; set; }
}
