namespace SystemMod.Models.SysDataScopeGroupDtos;

/// <see cref="SysDataScopeGroup"/>
public class SysDataScopeGroupFilterDto : FilterBase
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [MaxLength(30)]
    public string? Name { get; set; }

    public bool? IsEnabled { get; set; }
}
