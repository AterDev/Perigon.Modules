namespace SystemMod.Models.SysRoleDtos;

/// <summary>
/// 角色查询筛选
/// </summary>
/// <inheritdoc cref="SysRole"/>
public class SysRoleFilterDto : FilterBase
{
    /// <summary>
    /// 角色显示名称
    /// </summary>
    [MaxLength(30)]
    public string? Name { get; set; }

    /// <summary>
    /// 角色名，系统标识
    /// </summary>
    public string? NameValue { get; set; }
}
