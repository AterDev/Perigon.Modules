namespace SystemMod.Models.SysDataScopeGroupDtos;

/// <see cref="Entity.SystemMod.SysDataScopeGroup"/>
public class SysDataScopeGroupAddDto
{
    [MaxLength(30)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;
}
