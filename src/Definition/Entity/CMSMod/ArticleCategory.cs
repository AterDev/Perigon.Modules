namespace Entity.CMSMod;

/// <summary>
/// 目录
/// </summary>
public class ArticleCategory : EntityBase, ITreeNode<ArticleCategory>
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 层级
    /// </summary>
    public short Level { get; set; }

    /// <summary>
    /// 子目录
    /// </summary>
    [NotMapped]
    public List<ArticleCategory> Children { get; set; } = [];

    /// <summary>
    /// 父目录
    /// </summary>
    [NotMapped, ForeignKey(nameof(ParentId))]
    public ArticleCategory? Parent { get; set; }
    public Guid? ParentId { get; set; }
    [NotMapped]
    public ICollection<Article> Blogs { get; set; } = [];

    public Guid UserId { get; set; }
}
