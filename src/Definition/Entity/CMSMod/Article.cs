namespace Entity.CMSMod;

/// <summary>
/// 内容
/// </summary>
public class Article : EntityBase
{
    /// <summary>
    /// 标题
    /// </summary>
    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required, MaxLength(200000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 作者
    /// </summary>
    [Required, MaxLength(200)]
    public string Authors { get; set; } = string.Empty;

    /// <summary>
    /// 语言类型的持久化值。保留旧数据库的 LanguageType 列名。
    /// </summary>
    [JsonIgnore, Column("LanguageType")]
    public int LanguageTypeValue { get; set; } = (int)CMSMod.LanguageType.CN;

    /// <summary>
    /// 语言类型。
    /// </summary>
    [NotMapped]
    public LanguageType LanguageType
    {
        get => (LanguageType)LanguageTypeValue;
        set => LanguageTypeValue = (int)value;
    }

    /// <summary>
    /// 全站类别的持久化值。保留旧数据库的 ContentType 列名。
    /// </summary>
    [JsonIgnore, Column("ContentType")]
    public int ContentTypeValue { get; set; }

    /// <summary>
    /// 全站类别。
    /// </summary>
    [NotMapped]
    public ContentType ContentType
    {
        get => (ContentType)ContentTypeValue;
        set => ContentTypeValue = (int)value;
    }

    /// <summary>
    /// 是否审核
    /// </summary>
    public bool IsAudit { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否原创
    /// </summary>
    public bool IsOriginal { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// 所属目录
    /// </summary>
    [NotMapped, ForeignKey(nameof(CatalogId))]
    public ArticleCategory Catalog { get; set; } = null!;
    public Guid CatalogId { get; set; }

    /// <summary>
    /// 浏览量
    /// </summary>
    public int ViewCount { get; set; }
}

public enum ContentType
{
    /// <summary>
    /// News
    /// </summary>
    [Description("News")]
    News,

    /// <summary>
    /// ViewPoint
    /// </summary>
    [Description("ViewPoint")]
    ViewPoint,

    /// <summary>
    /// Knowledge
    /// </summary>
    [Description("Knowledge")]
    Knowledge,

    /// <summary>
    /// Documentary
    /// </summary>
    [Description("Documentary")]
    Documentary,

    /// <summary>
    /// Private
    /// </summary>
    [Description("Private")]
    Private,
}

public enum LanguageType
{
    /// <summary>
    /// zn-CN
    /// </summary>
    [Description("zh-CN")]
    CN,

    /// <summary>
    /// en-US
    /// </summary>
    [Description("en-US")]
    EN,
}
