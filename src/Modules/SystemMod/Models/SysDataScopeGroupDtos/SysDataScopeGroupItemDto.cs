namespace SystemMod.Models.SysDataScopeGroupDtos;

/// <see cref="Entity.SystemMod.SysDataScopeGroup"/>
public class SysDataScopeGroupItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsEnabled { get; set; }

}
