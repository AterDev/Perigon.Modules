namespace Entity.SystemMod;

/// <summary>
/// 系统菜单角色关联表
/// </summary>
[Index(nameof(RoleId), nameof(MenuId), IsUnique = true)]
public class SysMenuRole : EntityBase
{
    /// <summary>
    /// 菜单ID
    /// </summary>
    public Guid MenuId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 导航属性 - 菜单
    /// </summary>
    [ForeignKey(nameof(MenuId))]
    public SysMenu SysMenu { get; set; } = default!;

    /// <summary>
    /// 导航属性 - 角色
    /// </summary>
    [ForeignKey(nameof(RoleId))]
    public SysRole SysRole { get; set; } = default!;
}
