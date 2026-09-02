namespace ResourceMod.Models.UserResourceDtos;

/// <summary>
/// 用户资源更新结构。
/// </summary>
public class UserResourceUpdateDto
{
    /// <summary>
    /// 资源定义 ID。
    /// </summary>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// 用户资源可见性。
    /// </summary>
    public UserResourceStatus Status { get; set; }

    /// <summary>
    /// 按资源定义填写的属性值。
    /// </summary>
    public List<ResourceValueDto> Values { get; set; } = [];
}
