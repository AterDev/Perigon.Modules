namespace ResourceMod.Models.ResDefinitionPropertyDtos;

public class ResDefinitionPropertyUpdateDto
{
    [MaxLength(60)] public required string Name { get; set; }
    public ResValueType ValueType { get; set; }
    public bool IsRequired { get; set; }
    [Range(1, 1000)] public int MaxLength { get; set; } = 200;
}
