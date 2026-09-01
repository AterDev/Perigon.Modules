namespace Entity.ResourceMod;

/// <summary>
/// 个人资源的可见性和公开申请状态。
/// </summary>
public enum PersonalResourceStatus
{
    /// <summary>
    /// 仅资源所有者可见。
    /// </summary>
    [Description("Private")]
    Private,

    /// <summary>
    /// 申请公开。
    /// </summary>
    [Description("ApplyPublic")]
    ApplyPublic,

    /// <summary>
    /// 公开申请状态的兼容别名。
    /// </summary>
    Public = ApplyPublic
}
