namespace Entity.ResourceMod;

[Index(nameof(TenantId), nameof(RoleId), nameof(EnvironmentId), nameof(CategoryId), IsUnique = true)]
public class ResPermission : EntityBase
{
    public Guid RoleId { get; set; }
    public Guid EnvironmentId { get; set; }
    public Guid CategoryId { get; set; }
    [ForeignKey(nameof(EnvironmentId))]
    public ResEnvironment Environment { get; set; } = null!;
    [ForeignKey(nameof(CategoryId))]
    public ResCategory Category { get; set; } = null!;
}
