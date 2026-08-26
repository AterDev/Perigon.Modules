namespace CMSMod.Models.ArticleDtos;

/// <summary>
/// 文章更新时请求结构。
/// </summary>
/// <inheritdoc cref="Article"/>
public class ArticleUpdateDto
{
    /// <summary>
    /// 文章标题。
    /// </summary>
    [MaxLength(100)]
    public string Title { get; set; } = default!;

    /// <summary>
    /// 文章描述。
    /// </summary>
    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>
    /// 文章正文内容。
    /// </summary>
    [MaxLength(200000)]
    public string Content { get; set; } = default!;

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
    /// 文章语言类型；未提供时保留原值。
    /// </summary>
    public LanguageType? LanguageType { get; set; }

    /// <summary>
    /// 文章内容类型；未提供时保留原值。
    /// </summary>
    public ContentType? BlogType { get; set; }

    /// <summary>
    /// 是否公开文章；未提供时保留原值。
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// 是否为原创文章；未提供时保留原值。
    /// </summary>
    public bool? IsOriginal { get; set; }

    /// <summary>
    /// 所属目录 ID；未提供时保留原目录。
    /// </summary>
    public Guid? CatalogId { get; set; }
}
