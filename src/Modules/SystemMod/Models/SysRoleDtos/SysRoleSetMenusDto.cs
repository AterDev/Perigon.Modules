namespace SystemMod.Models.SysRoleDtos;

/// <summary>
/// 菜单更新
/// </summary>
/// <see cref="SysRole"/>
public class SysRoleSetMenusDto
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<Guid> MenuIds { get; set; } = [];
}
