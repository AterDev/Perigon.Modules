namespace Entity.AuditMod;

[Index(nameof(FlowKey), IsUnique = true)]
[Index(
    nameof(SceneCode),
    IsUnique = true,
    Filter = "\"is_enabled\" = true")]
public sealed class AuditFlow : EntityBase
{
    [StringLength(100), Required] public string FlowKey { get; set; } = string.Empty;
    [StringLength(100), Required] public string SceneCode { get; set; } = string.Empty;
    [StringLength(200), Required] public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

