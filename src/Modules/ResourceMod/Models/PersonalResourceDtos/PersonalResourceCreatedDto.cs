namespace ResourceMod.Models.PersonalResourceDtos;

/// <summary>
/// 个人资源创建成功响应结构。
/// </summary>
public class PersonalResourceCreatedDto
{
    /// <summary>
    /// 新创建个人资源的唯一标识。
    /// </summary>
    public Guid Id { get; set; }
}
