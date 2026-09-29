namespace Entity.SystemMod;

using Entity;

/// <summary>
/// 用户数据权限中间表
/// </summary>
[Index(nameof(UserId), nameof(DataScopeGroupId), IsUnique = true)]
public class SysUserDataScopeGroup : EntityBase
{
    public Guid UserId { get; set; }

    public Guid DataScopeGroupId { get; set; }

    [ForeignKey(nameof(UserId))]
    public SysUser User { get; set; } = default!;

    [ForeignKey(nameof(DataScopeGroupId))]
    public SysDataScopeGroup DataScopeGroup { get; set; } = default!;

}
