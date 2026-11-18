namespace SystemMod.Models.SysDataScopeDtos;

/// <summary>
/// 更新数据权限范围。
/// </summary>
public class SysDataScopeUpdateDto
{
    [StringLength(60)]
    public string? Name { get; set; }

    [StringLength(30)]
    public string? ResourceCode { get; set; }

    public List<Guid>? TargetIds { get; set; }

    public DataScopeType? ScopeType { get; set; }

    public Guid? GroupId { get; set; }
}
