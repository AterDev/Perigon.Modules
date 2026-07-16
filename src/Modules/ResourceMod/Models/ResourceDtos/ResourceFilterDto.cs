namespace ResourceMod.Models.ResourceDtos;

public class ResourceFilterDto : FilterBase
{
    public Guid? EnvironmentId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? DefinitionId { get; set; }
    public string? TagName { get; set; }
}
