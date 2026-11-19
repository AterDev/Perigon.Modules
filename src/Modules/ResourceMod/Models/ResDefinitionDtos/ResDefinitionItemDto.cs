using ResourceMod.Models.ResDefinitionPropertyDtos;
using ResourceMod.Models.ResourceDtos;

namespace ResourceMod.Models.ResDefinitionDtos;

/// <summary>资源定义响应结构。</summary>
public class ResDefinitionItemDto
{
    /// <summary>资源定义唯一标识。</summary>
    public Guid Id { get; set; }
    /// <summary>创建时间。</summary>
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>最后更新时间。</summary>
    public DateTimeOffset UpdatedTime { get; set; }
    /// <summary>是否已删除。</summary>
    public bool IsDeleted { get; set; }
    /// <summary>资源定义名称。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    public string? Icon { get; set; }
    /// <summary>定义包含的属性，按关联排序。</summary>
    public List<ResDefinitionPropertyItemDto> Properties { get; set; } = [];
    /// <summary>使用此定义的资源。配置接口当前不填充此集合。</summary>
    public List<ResourceItemDto> Resources { get; set; } = [];

    public static ResDefinitionItemDto FromEntity(
        ResDefinition entity,
        IEnumerable<ResDefinitionPropertyItemDto> properties)
    {
        return new ResDefinitionItemDto
        {
            Id = entity.Id,
            CreatedTime = entity.CreatedTime,
            UpdatedTime = entity.UpdatedTime,
            IsDeleted = entity.IsDeleted,
            Name = entity.Name,
            Icon = entity.Icon,
            Properties = properties.ToList(),
        };
    }
}
