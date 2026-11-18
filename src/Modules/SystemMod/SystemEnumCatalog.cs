using Entity;
using Entity.AuditMod;
using Entity.CMSMod;
using Entity.ResourceMod;

namespace SystemMod;

/// <summary>
/// The enum catalog exposed by the system configuration endpoint.
///
/// Keep this list explicit: the previous assembly scan was not compatible with
/// trimming or Native AOT and did not include enums outside Share.dll/Entity.dll.
/// </summary>
internal static class SystemEnumCatalog
{
    public static Dictionary<string, List<EnumDictionary>> Create()
    {
        return EnumHelper.CreateEnumInfo(
            (nameof(ContentType), EnumHelper.ToList<ContentType>(
                (ContentType.News, "News"),
                (ContentType.ViewPoint, "ViewPoint"),
                (ContentType.Knowledge, "Knowledge"),
                (ContentType.Documentary, "Documentary"),
                (ContentType.Private, "Private"))),
            (nameof(LanguageType), EnumHelper.ToList<LanguageType>(
                (LanguageType.CN, "zh-CN"),
                (LanguageType.EN, "en-US"))),
            (nameof(AuditMode), EnumHelper.ToList<AuditMode>()),
            (nameof(AuditRecordStatus), EnumHelper.ToList<AuditRecordStatus>()),
            (nameof(AuditStepStatus), EnumHelper.ToList<AuditStepStatus>()),
            (nameof(AuditTaskStatus), EnumHelper.ToList<AuditTaskStatus>()),
            (nameof(AuditDecision), EnumHelper.ToList<AuditDecision>()),
            (nameof(ResValueType), EnumHelper.ToList<ResValueType>(
                (ResValueType.String, "字符串"),
                (ResValueType.Number, "数字"),
                (ResValueType.Boolean, "布尔值"),
                (ResValueType.Date, "日期"),
                (ResValueType.Uri, "URI"),
                (ResValueType.IPAddress, "IP 地址"))),
            (nameof(UserResourceStatus), EnumHelper.ToList<UserResourceStatus>(
                (UserResourceStatus.Private, "Private"),
                (UserResourceStatus.ApplyPublic, "ApplyPublic"))),
            (nameof(UserResourceAuditStatus), EnumHelper.ToList<UserResourceAuditStatus>(
                (UserResourceAuditStatus.NotRequired, "NotRequired"),
                (UserResourceAuditStatus.Pending, "Pending"),
                (UserResourceAuditStatus.Approved, "Approved"),
                (UserResourceAuditStatus.Rejected, "Rejected"))),
            (nameof(DataScopeType), EnumHelper.ToList<DataScopeType>()),
            (nameof(MenuType), EnumHelper.ToList<MenuType>(
                (MenuType.Page, "页面"),
                (MenuType.Button, "按钮"))),
            (nameof(Sex), EnumHelper.ToList<Sex>(
                (Sex.Male, "男性"),
                (Sex.Female, "女性"),
                (Sex.Else, "其他")))
        );
    }
}
