namespace SystemMod.Models.SysUserDtos;

/// <summary>
/// 系统用户查询筛选
/// </summary>
/// <inheritdoc cref="SysUser"/>
public class SysUserFilterDto : FilterBase
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(30)]
    public string? UserName { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    public Guid? RoleId { get; set; }
}
