namespace Entity.ResourceMod;

[Index(nameof(ResourceId), nameof(DefinitionPropertyId), IsUnique = true)]
public class ResValue : EntityBase
{
    public Guid ResourceId { get; set; }
    public Guid DefinitionPropertyId { get; set; }
    [MaxLength(1000)]
    public required string Value { get; set; }
    [MaxLength(60)]
    public required string PropertyNameSnapshot { get; set; }
    public ResValueType ValueTypeSnapshot { get; set; }
    [ForeignKey(nameof(ResourceId))]
    public Resource Resource { get; set; } = null!;
    [ForeignKey(nameof(DefinitionPropertyId))]
    public ResDefinitionProperty DefinitionProperty { get; set; } = null!;
}
