namespace ResourceMod.Models.ResourceDtos;

/// <summary>
/// 资源列表项响应结构。
/// </summary>
/// <inheritdoc cref="Resource"/>
public class ResourceItemDto
{
    /// <summary>
    /// 资源唯一标识。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 资源所属环境 ID。
    /// </summary>
    public Guid EnvironmentId { get; set; }

    /// <summary>
    /// 资源所属环境名称。
    /// </summary>
    public required string EnvironmentName { get; set; }

    /// <summary>
    /// 资源所属分类 ID。
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// 资源所属分类名称。
    /// </summary>
    public required string CategoryName { get; set; }

    /// <summary>
    /// 资源所属分组 ID，可为空。
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 资源所属分组名称，可为空。
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// 资源使用的定义 ID。
    /// </summary>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// 资源定义名称。
    /// </summary>
    public required string DefinitionName { get; set; }

    /// <summary>
    /// 资源标签名称列表。
    /// </summary>
    public List<string> TagNames { get; set; } = [];

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public DateTimeOffset UpdatedTime { get; set; }
}
