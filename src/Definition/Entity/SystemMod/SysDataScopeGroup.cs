namespace Entity.SystemMod;

/// <summary>
/// 数据权限组
/// </summary>
public class SysDataScopeGroup : EntityBase
{
    /// <summary>
    /// 分组名称
    /// </summary>
    [StringLength(30)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 权限说明
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    [NotMapped]
    public ICollection<SysDataScope> DataScopes { get; set; } = [];

    /// <summary>
    /// 数据权限关联表
    /// </summary>
    [NotMapped]
    public ICollection<SysUserDataScopeGroup> Users { get; set; } = [];

}
