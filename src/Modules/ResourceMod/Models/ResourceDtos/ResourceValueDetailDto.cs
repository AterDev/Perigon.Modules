namespace ResourceMod.Models.ResourceDtos;

public class ResourceValueDetailDto
{
    public Guid DefinitionPropertyId { get; set; }
    public required string Name { get; set; }
    public ResValueType ValueType { get; set; }
    public required string Value { get; set; }
}
