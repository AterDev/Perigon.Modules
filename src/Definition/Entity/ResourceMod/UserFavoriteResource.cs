namespace Entity.ResourceMod;

/// <summary>
/// 用户收藏的常规资源。
/// </summary>
[Index(nameof(UserId), nameof(ResourceId), IsUnique = true)]
public class UserFavoriteResource : EntityBase
{
    /// <summary>
    /// 收藏用户 ID；不建立用户实体导航，避免跨模块耦合。
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 被收藏的公开常规资源 ID。
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// 被收藏资源及其属性值。
    /// </summary>
    [ForeignKey(nameof(ResourceId))]
    public Resource Resource { get; set; } = null!;
}
