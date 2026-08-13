namespace Entity.ResourceMod;

/// <summary>资源属性定义配置。</summary>
public class ResDefinition : EntityBase
{
    /// <summary>资源定义名称。</summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    [MaxLength(60)]
    public string? Icon { get; set; }
    /// <summary>定义与属性的关联。</summary>
    [JsonIgnore]
    public ICollection<ResDefinitionPropertyMap> PropertyMaps { get; set; } = [];
    /// <summary>定义包含的属性。由管理器按关联排序后填充，用于 API 响应。</summary>
    [NotMapped]
    public ICollection<ResDefinitionProperty> Properties { get; set; } = [];
    /// <summary>使用此定义的资源。</summary>
    public ICollection<Resource> Resources { get; set; } = [];
}
