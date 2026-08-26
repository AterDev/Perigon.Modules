namespace ResourceMod.Models.ResourceDtos;

/// <summary>
/// 资源列表查询筛选条件。
/// </summary>
/// <inheritdoc cref="Resource"/>
public class ResourceFilterDto : FilterBase
{
    /// <summary>
    /// 按环境 ID 筛选。
    /// </summary>
    public Guid? EnvironmentId { get; set; }

    /// <summary>
    /// 按分类 ID 筛选。
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// 按分组 ID 筛选。
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 按资源定义 ID 筛选。
    /// </summary>
    public Guid? DefinitionId { get; set; }

    /// <summary>
    /// 按标签名称筛选。
    /// </summary>
    public string? TagName { get; set; }

    /// <summary>
    /// 在资源定义名称、标签名称和属性值中进行关键字搜索；少于两个字符时不执行关键字搜索。
    /// </summary>
    public string? SearchKey { get; set; }
}
