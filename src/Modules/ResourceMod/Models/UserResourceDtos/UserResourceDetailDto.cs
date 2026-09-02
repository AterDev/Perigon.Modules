namespace ResourceMod.Models.UserResourceDtos;

/// <summary>
/// 用户资源详情结构。
/// </summary>
public class UserResourceDetailDto : UserResourceItemDto
{
    /// <summary>
    /// 用户资源属性值列表。
    /// </summary>
    public List<ResourceValueDetailDto> Values { get; set; } = [];
}
