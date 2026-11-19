namespace Entity.AuditMod;

[Index(nameof(FlowId), nameof(StepNo), IsUnique = true)]
public sealed class AuditStepDefinition : EntityBase
{
    public Guid FlowId { get; set; }
    public int StepNo { get; set; }
    [StringLength(200), Required] public string Name { get; set; } = string.Empty;
    public Guid[] AuditorUserIds { get; set; } = [];
    [Column("AuditModeValue")]
    public AuditMode AuditMode { get; set; }
}

