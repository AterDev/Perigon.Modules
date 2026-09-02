namespace ResourceMod.Models.UserFavoriteResourceDtos;

/// <summary>
/// 收藏资源成功后的响应。
/// </summary>
public class UserFavoriteResourceCreatedDto
{
    /// <summary>
    /// 收藏记录 ID。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 被收藏的常规资源 ID。
    /// </summary>
    public Guid ResourceId { get; set; }
}
