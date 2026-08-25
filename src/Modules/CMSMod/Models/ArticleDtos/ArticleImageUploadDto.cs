namespace CMSMod.Models.ArticleDtos;

/// <summary>
/// 文章图片上传结果。
/// </summary>
public class ArticleImageUploadDto
{
    /// <summary>
    /// 可写入 Markdown 的根相对路径。
    /// </summary>
    public required string Path { get; init; }
}
