using ResourceMod.Models.ResourceDtos;

namespace ResourceMod.Models.UserFavoriteResourceDtos;

/// <summary>
/// 我的收藏资源列表项。
/// </summary>
public class UserFavoriteResourceItemDto
{
    /// <summary>
    /// 收藏记录 ID。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 被收藏的常规资源 ID。
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// 收藏时间。
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; }

    /// <summary>
    /// 被收藏资源摘要。
    /// </summary>
    public required ResourceItemDto Resource { get; set; }
}
