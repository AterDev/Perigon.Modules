namespace ResourceMod.Models.ResDefinitionDtos;

public class ResDefinitionAddDto
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
    public List<ResDefinitionPropertyDto> Properties { get; set; } = [];
}
