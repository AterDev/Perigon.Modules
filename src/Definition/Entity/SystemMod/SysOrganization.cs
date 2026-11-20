namespace Entity.SystemMod;

/// <summary>
/// 组织结构
/// </summary>
public class SysOrganization : EntityBase, ITreeNode<SysOrganization>
{
    /// <summary>
    /// 名称
    /// </summary>
    [StringLength(100)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 外部系统中的组织标识。
    /// </summary>
    [StringLength(40)]
    public string? ExternalId { get; set; }

    /// <summary>
    /// 子目录
    /// </summary>
    public List<SysOrganization> Children { get; set; } = [];

    /// <summary>
    /// 父目录
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    public SysOrganization? Parent { get; set; }
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 组织用户关联
    /// </summary>
    public ICollection<SysUserOrganization> SysUserOrganizations { get; set; } = [];
}
