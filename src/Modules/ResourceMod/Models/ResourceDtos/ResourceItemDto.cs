namespace ResourceMod.Models.ResourceDtos;

public class ResourceItemDto
{
    public Guid Id { get; set; }
    public Guid EnvironmentId { get; set; }
    public required string EnvironmentName { get; set; }
    public Guid CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    public Guid DefinitionId { get; set; }
    public required string DefinitionName { get; set; }
    public List<string> TagNames { get; set; } = [];
    public DateTimeOffset UpdatedTime { get; set; }
}
