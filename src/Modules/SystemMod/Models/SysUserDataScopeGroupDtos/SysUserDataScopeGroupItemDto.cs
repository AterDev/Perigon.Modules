using SystemMod.Models.SysDataScopeDtos;

namespace SystemMod.Models.SysUserDataScopeGroupDtos;

/// <summary>
/// 当前用户的数据权限组及其范围。
/// </summary>
public class SysUserDataScopeGroupItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<SysDataScopeItemDto> DataScopes { get; set; } = [];
}
