namespace ResourceMod.Models.ResourceDtos;

/// <summary>
/// 资源新增请求结构。
/// </summary>
/// <inheritdoc cref="Resource"/>
public class ResourceAddDto
{
    /// <summary>
    /// 资源所属环境 ID。
    /// </summary>
    public Guid EnvironmentId { get; set; }

    /// <summary>
    /// 资源所属分类 ID。
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// 资源所属分组 ID，可选；分组必须属于所选分类。
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 资源使用的定义 ID。
    /// </summary>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// 资源标签名称列表，允许为空。
    /// </summary>
    public List<string> TagNames { get; set; } = [];

    /// <summary>
    /// 按资源定义填写的属性值列表。
    /// </summary>
    public List<ResourceValueDto> Values { get; set; } = [];
}
