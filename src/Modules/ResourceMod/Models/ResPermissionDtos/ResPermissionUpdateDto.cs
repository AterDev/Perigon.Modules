namespace ResourceMod.Models.ResPermissionDtos;

public class ResPermissionUpdateDto
{
    public Guid EnvironmentId { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid> RoleIds { get; set; } = [];
}
