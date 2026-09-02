namespace Entity.ResourceMod;

/// <summary>
/// 用户资源的可见性。
/// </summary>
public enum UserResourceStatus
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
    ApplyPublic
}
