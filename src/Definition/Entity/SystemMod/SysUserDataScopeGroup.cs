namespace Entity.SystemMod;

using Entity;

/// <summary>
/// 用户数据权限中间表
/// </summary>
public class SysUserDataScopeGroup : EntityBase
{
    public Guid UserId { get; set; }

    public Guid DataScopeGroupId { get; set; }

    [ForeignKey(nameof(UserId))]
    [NotMapped]
    public SysUser User { get; set; } = default!;

    [ForeignKey(nameof(DataScopeGroupId))]
    [NotMapped]
    public SysDataScopeGroup DataScopeGroup { get; set; } = default!;

}
