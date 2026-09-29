namespace SystemMod.Models.SysDataScopeGroupDtos;

using SystemMod.Models.SysDataScopeDtos;

/// <see cref="Entity.SystemMod.SysDataScopeGroup"/>
public class SysDataScopeGroupDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsEnabled { get; set; }

    public List<SysDataScopeItemDto> DataScopes { get; set; } = [];

    public List<Guid> UserIds { get; set; } = [];
}
