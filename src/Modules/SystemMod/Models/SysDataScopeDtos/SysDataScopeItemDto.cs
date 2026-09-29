namespace SystemMod.Models.SysDataScopeDtos;

/// <summary>
/// 数据权限范围列表项。
/// </summary>
public class SysDataScopeItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ResourceCode { get; set; } = string.Empty;

    public List<Guid> TargetIds { get; set; } = [];

    public DataScopeType ScopeType { get; set; }

    public Guid GroupId { get; set; }
}
