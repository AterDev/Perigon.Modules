namespace ResourceMod.Models.ResourceDtos;

/// <summary>
/// 资源创建成功响应结构。
/// </summary>
public class ResourceCreatedDto
{
    /// <summary>
    /// 新创建资源的唯一标识。
    /// </summary>
    public Guid Id { get; set; }
}
