using System.Text.Json.Serialization;

namespace Entity.SystemMod;

/// <summary>
/// 权限
/// </summary>
public class SysDataScope : EntityBase
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [StringLength(60)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数据标识Code
    /// </summary>
    [StringLength(30)]
    [Required]
    public string ResourceCode { get; set; } = string.Empty;

    /// <summary>
    /// 允许访问的数据标识
    /// </summary>
    public List<Guid> TargetIds { get; set; } = [];

    /// <summary>
    /// 权限类型
    /// </summary>
    [Column("ScopeType")]
    public DataScopeType ScopeType { get; set; }

    /// <summary>
    /// 权限组
    /// </summary>
    [ForeignKey(nameof(GroupId))]
    [NotMapped]
    public SysDataScopeGroup Group { get; set; } = null!;

    public Guid GroupId { get; set; } = default!;
}

/// <summary>
/// 权限范围类型
/// </summary>
public enum DataScopeType
{
    None,
    /// <summary>
    /// 全部数据
    /// </summary>
    All,

    /// <summary>
    /// 包含的targetIds
    /// </summary>
    Include
}
