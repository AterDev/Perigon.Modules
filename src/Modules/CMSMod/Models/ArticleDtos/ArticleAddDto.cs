namespace CMSMod.Models.ArticleDtos;

/// <summary>
/// 文章添加时请求结构。
/// </summary>
/// <inheritdoc cref="Article"/>
public class ArticleAddDto
{
    /// <summary>
    /// 文章标题。
    /// </summary>
    [MaxLength(100)]
    public required string Title { get; set; }

    /// <summary>
    /// 文章描述。
    /// </summary>
    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>
    /// 文章正文内容。
    /// </summary>
    [MaxLength(200000)]
    public required string Content { get; set; }

    /// <summary>
    /// 翻译后的文章标题。
    /// </summary>
    [MaxLength(200)]
    public string? TranslateTitle { get; set; }

    /// <summary>
    /// 翻译后的文章正文内容。
    /// </summary>
    [MaxLength(12000)]
    public string? TranslateContent { get; set; }

    /// <summary>
    /// 文章语言类型。
    /// </summary>
    public LanguageType LanguageType { get; set; } = LanguageType.CN;

    /// <summary>
    /// 文章内容类型。
    /// </summary>
    public ContentType BlogType { get; set; }

    /// <summary>
    /// 是否公开文章。
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否为原创文章。
    /// </summary>
    public bool IsOriginal { get; set; }

    /// <summary>
    /// 所属目录 ID。
    /// </summary>
    public Guid CatalogId { get; set; }
}
