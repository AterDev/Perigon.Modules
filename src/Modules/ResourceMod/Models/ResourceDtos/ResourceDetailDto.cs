namespace ResourceMod.Models.ResourceDtos;

public class ResourceDetailDto : ResourceItemDto
{
    public List<ResourceValueDetailDto> Values { get; set; } = [];
}
