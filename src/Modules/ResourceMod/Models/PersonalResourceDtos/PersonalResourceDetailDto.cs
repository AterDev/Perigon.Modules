namespace ResourceMod.Models.PersonalResourceDtos;

/// <summary>
/// 个人资源详情响应结构。
/// </summary>
public class PersonalResourceDetailDto : PersonalResourceItemDto
{
    /// <summary>
    /// 个人资源属性值列表。
    /// </summary>
    public List<ResourceValueDetailDto> Values { get; set; } = [];
}
