namespace ResourceMod.Models.ResDefinitionPropertyDtos;

/// <summary>资源属性响应结构。</summary>
public class ResDefinitionPropertyItemDto
{
    /// <summary>资源属性唯一标识。</summary>
    public Guid Id { get; set; }
    /// <summary>创建时间。</summary>
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>最后更新时间。</summary>
    public DateTimeOffset UpdatedTime { get; set; }
    /// <summary>是否已删除。</summary>
    public bool IsDeleted { get; set; }
    /// <summary>属性名称。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>属性值类型。</summary>
    public ResValueType ValueType { get; set; }
    /// <summary>是否必填。</summary>
    public bool IsRequired { get; set; }
    /// <summary>属性值最大长度。</summary>
    public int MaxLength { get; set; }
    /// <summary>属性在资源定义中的显示排序。</summary>
    public int Sort { get; set; }

    public static ResDefinitionPropertyItemDto FromEntity(ResDefinitionProperty entity, int sort = 0)
    {
        return new ResDefinitionPropertyItemDto
        {
            Id = entity.Id,
            CreatedTime = entity.CreatedTime,
            UpdatedTime = entity.UpdatedTime,
            IsDeleted = entity.IsDeleted,
            Name = entity.Name,
            ValueType = entity.ValueType,
            IsRequired = entity.IsRequired,
            MaxLength = entity.MaxLength,
            Sort = sort,
        };
    }
}
