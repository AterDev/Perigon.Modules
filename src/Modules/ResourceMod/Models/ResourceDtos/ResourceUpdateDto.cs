namespace ResourceMod.Models.ResourceDtos;

public class ResourceUpdateDto
{
    public Guid EnvironmentId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid DefinitionId { get; set; }
    public List<string> TagNames { get; set; } = [];
    public List<ResourceValueDto> Values { get; set; } = [];
}
