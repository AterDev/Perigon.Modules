namespace ResourceMod.Models.ResDefinitionDtos;

public class ResDefinitionPropertyDto
{
    public Guid? Id { get; set; }
    [MaxLength(60)] public required string Name { get; set; }
    public ResValueType ValueType { get; set; }
    public bool IsRequired { get; set; }
    [Range(1, 1000)] public int MaxLength { get; set; } = 200;
    public int Sort { get; set; }
}
