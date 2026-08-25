namespace CMSMod.Models.ArticleDtos;

/// <summary>
/// 博客添加时请求结构
/// </summary>
/// <inheritdoc cref="Article"/>
public class ArticleAddDto
{
    /// <summary>
    /// 标题
    /// </summary>
    [MaxLength(100)]
    public required string Title { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(200000)]
    public required string Content { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [MaxLength(200)]
    public string? TranslateTitle { get; set; }

    /// <summary>
    /// 翻译内容
    /// </summary>
    [MaxLength(12000)]
    public string? TranslateContent { get; set; }

    /// <summary>
    /// 语言类型
    /// </summary>
    public LanguageType LanguageType { get; set; } = LanguageType.CN;

    /// <summary>
    /// 全站类别
    /// </summary>
    public ContentType BlogType { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否原创
    /// </summary>
    public bool IsOriginal { get; set; }
    public Guid CatalogId { get; set; }
}
