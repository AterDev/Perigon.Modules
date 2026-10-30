namespace Entity.SystemMod;

/// <summary>
/// 系统用户角色关联表
/// </summary>
public class SysUserRole : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 导航属性 - 用户
    /// </summary>
    [ForeignKey(nameof(UserId))]
    [NotMapped]
    public SysUser User { get; set; } = default!;

    /// <summary>
    /// 导航属性 - 角色
    /// </summary>
    [ForeignKey(nameof(RoleId))]
    [NotMapped]
    public SysRole Role { get; set; } = default!;
}
