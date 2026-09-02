namespace ResourceMod.Models.UserResourceDtos;

/// <summary>
/// 用户资源查询筛选条件。
/// </summary>
public class UserResourceFilterDto : FilterBase
{
    /// <summary>
    /// 按用户资源状态筛选。
    /// </summary>
    public UserResourceStatus? Status { get; set; }

    /// <summary>
    /// 按审核状态筛选。
    /// </summary>
    public UserResourceAuditStatus? AuditStatus { get; set; }
}
