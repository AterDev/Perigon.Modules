namespace ResourceMod.Models.UserFavoriteResourceDtos;

/// <summary>
/// 新增用户收藏资源请求。
/// </summary>
public class UserFavoriteResourceAddDto
{
    /// <summary>
    /// 要收藏的常规资源 ID。
    /// </summary>
    public Guid ResourceId { get; set; }
}
