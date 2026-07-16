namespace ResourceMod.Models.ResourceDtos;

public class ResourceValueDto
{
    public Guid DefinitionPropertyId { get; set; }
    [MaxLength(1000)] public required string Value { get; set; }
}
