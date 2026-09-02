namespace ResourceMod.Models.UserResourceDtos;

/// <summary>
/// 用户资源新增结构。
/// </summary>
public class UserResourceAddDto
{
    /// <summary>
    /// 资源定义 ID。
    /// </summary>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// 资源可见性。Private 表示仅自己可见，ApplyPublic 表示申请公开。
    /// </summary>
    public UserResourceStatus Status { get; set; }

    /// <summary>
    /// 按资源定义填写的属性值。
    /// </summary>
    public List<ResourceValueDto> Values { get; set; } = [];
}
