namespace Entity.SystemMod;

[Index(nameof(RoleId), nameof(MenuId), IsUnique = true)]
public class SysMenuRole : EntityBase
{
    public Guid MenuId { get; set; }
    public Guid RoleId { get; set; }

    public SysMenu SysMenu { get; set; } = default!;
    public SysRole SysRole { get; set; } = default!;
}
