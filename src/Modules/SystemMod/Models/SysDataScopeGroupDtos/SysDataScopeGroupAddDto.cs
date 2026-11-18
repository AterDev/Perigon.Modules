namespace SystemMod.Models.SysDataScopeGroupDtos;

/// <see cref="Entity.SystemMod.SysDataScopeGroup"/>
public class SysDataScopeGroupAddDto
{
    [StringLength(30)]
    public required string Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;
}
