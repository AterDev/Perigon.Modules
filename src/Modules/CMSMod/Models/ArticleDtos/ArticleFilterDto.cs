namespace CMSMod.Models.ArticleDtos;

/// <summary>
/// 文章查询筛选。
/// </summary>
/// <inheritdoc cref="Article"/>
public class ArticleFilterDto : FilterBase
{
    /// <summary>
    /// 按文章标题筛选。
    /// </summary>
    [MaxLength(100)]
    public string? Title { get; set; }

    /// <summary>
    /// 按文章描述筛选。
    /// </summary>
    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>
    /// 按作者名称筛选。
    /// </summary>
    [MaxLength(200)]
    public string? Authors { get; set; }

    /// <summary>
    /// 按翻译后的文章标题筛选。
    /// </summary>
    [MaxLength(200)]
    public string? TranslateTitle { get; set; }

    /// <summary>
    /// 按文章语言类型筛选。
    /// </summary>
    public LanguageType? LanguageType { get; set; }

    /// <summary>
    /// 按文章内容类型筛选。
    /// </summary>
    public ContentType? BlogType { get; set; }

    /// <summary>
    /// 按是否审核筛选。
    /// </summary>
    public bool? IsAudit { get; set; }

    /// <summary>
    /// 按是否公开筛选。
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// 按是否原创筛选。
    /// </summary>
    public bool? IsOriginal { get; set; }

    /// <summary>
    /// 按作者用户 ID 筛选。
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 按所属目录 ID 筛选。
    /// </summary>
    public Guid? CatalogId { get; set; }

    /// <summary>
    /// 按浏览量筛选。
    /// </summary>
    public int? ViewCount { get; set; }
}
