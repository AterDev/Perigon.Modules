namespace Entity.ResourceMod;

/// <summary>
/// 用户资源公开申请的审核状态。
/// </summary>
public enum UserResourceAuditStatus
{
    /// <summary>
    /// 私有资源无需审核。
    /// </summary>
    [Description("NotRequired")]
    NotRequired,

    /// <summary>
    /// 等待管理员审核。
    /// </summary>
    [Description("Pending")]
    Pending,

    /// <summary>
    /// 审核通过。
    /// </summary>
    [Description("Approved")]
    Approved,

    /// <summary>
    /// 审核驳回。
    /// </summary>
    [Description("Rejected")]
    Rejected
}
