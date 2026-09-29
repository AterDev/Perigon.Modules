namespace Entity.SystemMod;

/// <summary>
/// 系统用户组织关联表
/// </summary>
[Index(nameof(UserId), nameof(OrganizationId), IsUnique = true)]
public class SysUserOrganization : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 组织ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 导航属性 - 用户
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public SysUser User { get; set; } = default!;

    /// <summary>
    /// 导航属性 - 组织
    /// </summary>
    [ForeignKey(nameof(OrganizationId))]
    public SysOrganization Organization { get; set; } = default!;
}
