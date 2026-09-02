namespace ResourceMod.Models.PersonalResourceDtos;

/// <summary>
/// 个人资源更新请求结构。
/// </summary>
public class PersonalResourceUpdateDto
{
    /// <summary>
    /// 资源定义 ID。
    /// </summary>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// 个人资源状态。
    /// </summary>
    public PersonalResourceStatus Status { get; set; }

    /// <summary>
    /// 按资源定义填写的属性值。
    /// </summary>
    public List<ResourceValueDto> Values { get; set; } = [];
}
