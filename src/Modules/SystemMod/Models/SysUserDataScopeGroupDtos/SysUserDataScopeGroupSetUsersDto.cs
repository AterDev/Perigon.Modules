namespace SystemMod.Models.SysUserDataScopeGroupDtos;

/// <summary>
/// 完整替换数据权限组中的用户。
/// </summary>
public class SysUserDataScopeGroupSetUsersDto
{
    public List<Guid> UserIds { get; set; } = [];
}
