namespace ResourceMod.Models.UserResourceDtos;

/// <summary>
/// 公开申请审核通过时补充的常规资源信息。
/// </summary>
public class UserResourceReviewDto
{
    /// <summary>
    /// 资源所属环境 ID。
    /// </summary>
    public Guid EnvironmentId { get; set; }

    /// <summary>
    /// 资源所属分类 ID。
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// 资源所属分组 ID，可选。
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 资源标签名称列表。
    /// </summary>
    public List<string> TagNames { get; set; } = [];

    /// <summary>
    /// 审核意见。
    /// </summary>
    [MaxLength(500)]
    public string? ReviewComment { get; set; }
}
