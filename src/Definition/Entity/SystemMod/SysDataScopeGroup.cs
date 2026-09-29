namespace Entity.SystemMod;

/// <summary>
/// 数据权限组
/// </summary>
[Index(nameof(Name))]
public class SysDataScopeGroup : EntityBase
{
    /// <summary>
    /// 分组名称
    /// </summary>
    [MaxLength(30)]
    public required string Name { get; set; }

    /// <summary>
    /// 权限说明
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public ICollection<SysDataScope> DataScopes { get; set; } = [];

    /// <summary>
    /// 数据权限关联表
    /// </summary>
    public ICollection<SysUserDataScopeGroup> Users { get; set; } = [];

}
