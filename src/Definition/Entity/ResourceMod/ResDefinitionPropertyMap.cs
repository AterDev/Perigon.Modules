namespace Entity.ResourceMod;

/// <summary>资源定义与资源属性的关联。</summary>
[Index(nameof(DefinitionId), nameof(PropertyId), IsUnique = true)]
public class ResDefinitionPropertyMap : EntityBase
{
    /// <summary>资源定义 ID。</summary>
    public Guid DefinitionId { get; set; }
    /// <summary>资源属性 ID。</summary>
    public Guid PropertyId { get; set; }
    /// <summary>属性在资源定义中的显示排序。</summary>
    public int Sort { get; set; }
    /// <summary>所属资源定义。</summary>
    [ForeignKey(nameof(DefinitionId))]
    [JsonIgnore]
    public ResDefinition Definition { get; set; } = null!;
    /// <summary>关联的资源属性。</summary>
    [ForeignKey(nameof(PropertyId))]
    [JsonIgnore]
    public ResDefinitionProperty Property { get; set; } = null!;
}
