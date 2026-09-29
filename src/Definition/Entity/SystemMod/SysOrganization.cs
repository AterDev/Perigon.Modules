namespace Entity.SystemMod;

/// <summary>
/// 组织结构
/// </summary>
[Index(nameof(Name))]
public class SysOrganization : EntityBase, ITreeNode<SysOrganization>
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

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
    public ICollection<SysUser> Users { get; set; } = [];
}
