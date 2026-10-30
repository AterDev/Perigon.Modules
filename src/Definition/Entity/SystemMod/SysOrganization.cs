namespace Entity.SystemMod;

/// <summary>
/// 组织结构
/// </summary>
public class SysOrganization : EntityBase, ITreeNode<SysOrganization>
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 子目录
    /// </summary>
    [NotMapped]
    public List<SysOrganization> Children { get; set; } = [];

    /// <summary>
    /// 父目录
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    [NotMapped]
    public SysOrganization? Parent { get; set; }
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 组织用户关联
    /// </summary>
    [NotMapped]
    public ICollection<SysUserOrganization> SysUserOrganizations { get; set; } = [];
}
