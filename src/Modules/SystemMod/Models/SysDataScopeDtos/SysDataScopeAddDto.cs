namespace SystemMod.Models.SysDataScopeDtos;

/// <summary>
/// 新增数据权限范围。
/// </summary>
public class SysDataScopeAddDto
{
    [MaxLength(60)]
    public required string Name { get; set; }

    [MaxLength(30)]
    public required string ResourceCode { get; set; }

    public List<Guid> TargetIds { get; set; } = [];

    public DataScopeType ScopeType { get; set; }

    public required Guid GroupId { get; set; }
}
