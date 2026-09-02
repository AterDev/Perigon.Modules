using ResourceMod.Models.ResourceDtos;

namespace ResourceMod.Models.UserFavoriteResourceDtos;

/// <summary>
/// 用户收藏资源详情。
/// </summary>
public class UserFavoriteResourceDetailDto
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
    /// 被收藏资源及其属性值。
    /// </summary>
    public required ResourceDetailDto Resource { get; set; }
}
