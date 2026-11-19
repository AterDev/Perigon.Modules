namespace Entity.ResourceMod;

/// <summary>资源实例的属性值。</summary>
public class ResValue : EntityBase
{
    /// <summary>资源 ID。</summary>
    public Guid ResourceId { get; set; }
    /// <summary>资源定义属性 ID。</summary>
    public Guid DefinitionPropertyId { get; set; }
    /// <summary>以字符串形式持久化的属性值。</summary>
    [StringLength(1000)]
    public string Value { get; set; } = string.Empty;
    /// <summary>保存时的属性名称快照。</summary>
    [StringLength(60)]
    public string PropertyNameSnapshot { get; set; } = string.Empty;
    /// <summary>保存时的属性值类型快照。</summary>
    public ResValueType ValueTypeSnapshot { get; set; }
    /// <summary>所属资源。</summary>
    [ForeignKey(nameof(ResourceId))]
    public Resource Resource { get; set; } = null!;
    /// <summary>所属资源定义属性。</summary>
    [ForeignKey(nameof(DefinitionPropertyId))]
    public ResDefinitionProperty DefinitionProperty { get; set; } = null!;
}
