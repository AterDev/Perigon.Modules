namespace CMSMod.Models.ArticleDtos;

/// <summary>
/// 文章详情。
/// </summary>
/// <inheritdoc cref="Article"/>
public class ArticleDetailDto
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
    public string Content { get; set; } = default!;

    /// <summary>
    /// 作者名称。
    /// </summary>
    [MaxLength(200)]
    public string Authors { get; set; } = default!;

    /// <summary>
    /// 翻译后的文章标题。
    /// </summary>
    [MaxLength(200)]
    public string? TranslateTitle { get; set; }

    /// <summary>
    /// 文章语言类型。
    /// </summary>
    public LanguageType LanguageType { get; set; } = LanguageType.CN;

    /// <summary>
    /// 文章内容类型。
    /// </summary>
    public ContentType BlogType { get; set; }

    /// <summary>
    /// 是否已审核。
    /// </summary>
    public bool IsAudit { get; set; }

    /// <summary>
    /// 是否公开文章。
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否为原创文章。
    /// </summary>
    public bool IsOriginal { get; set; }

    /// <summary>
    /// 作者用户 ID。
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 所属目录。
    /// </summary>
    public ArticleCategory Catalog { get; set; } = default!;

    /// <summary>
    /// 所属目录 ID。
    /// </summary>
    public Guid CatalogId { get; set; }

    /// <summary>
    /// 浏览量。
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// 文章唯一标识。
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
}
