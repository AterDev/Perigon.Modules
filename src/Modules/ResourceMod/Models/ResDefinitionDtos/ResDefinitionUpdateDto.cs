namespace ResourceMod.Models.ResDefinitionDtos;

public class ResDefinitionUpdateDto
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
    public List<ResDefinitionPropertyDto> Properties { get; set; } = [];
}
