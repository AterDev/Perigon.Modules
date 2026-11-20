using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Perigon.AspNetCore.Models;

// <perigon-aot-json:usings>
using AuditMod.Models.AuditFlowDtos;
using AuditMod.Models.AuditRecordDtos;
using AuditMod.Models.AuditTaskDtos;
using CMSMod.Models.ArticleCategoryDtos;
using CMSMod.Models.ArticleDtos;
using Entity.CMSMod;
using Entity.ResourceMod;
using Entity.SystemMod;
using NotificationMod.Models.InAppNotificationDtos;
using NotificationMod.Models.NotificationDeliveryDtos;
using NotificationMod.Models.NotificationEventDtos;
using NotificationMod.Models.NotificationReceiverDtos;
using NotificationMod.Models.NotificationRecoveryDtos;
using NotificationMod.Models.NotificationRuleDtos;
using NotificationMod.Models.NotificationSourceDtos;
using NotificationMod.Models.WebhookDtos;
using NotificationMod.Services;
using Perigon.AspNetCore.Utils;
using ResourceMod.Models.ResCategoryDtos;
using ResourceMod.Models.ResDefinitionDtos;
using ResourceMod.Models.ResDefinitionPropertyDtos;
using ResourceMod.Models.ResEnvironmentDtos;
using ResourceMod.Models.ResGroupDtos;
using ResourceMod.Models.ResPermissionDtos;
using ResourceMod.Models.ResTagDtos;
using ResourceMod.Models.ResourceDtos;
using ResourceMod.Models.UserFavoriteResourceDtos;
using ResourceMod.Models.UserResourceDtos;
using Share.Models.Auth;
using Share;
using System.Collections.Generic;
using System.Text.Json;
using System;
using SystemMod.Models.SysConfigDtos;
using SystemMod.Models.SysDataScopeDtos;
using SystemMod.Models.SysDataScopeGroupDtos;
using SystemMod.Models.SysLogsDtos;
using SystemMod.Models.SysMenuDtos;
using SystemMod.Models.SysOrganizationDtos;
using SystemMod.Models.SysRoleDtos;
using SystemMod.Models.SysUserDataScopeGroupDtos;
using SystemMod.Models.SysUserDtos;
using SystemMod.Models;
// </perigon-aot-json:usings>

namespace ApiService;

// <perigon-aot-json:generated>
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(AuditFlowAddDto))]
[JsonSerializable(typeof(AuditFlowDetailDto))]
[JsonSerializable(typeof(AuditFlowEnabledUpdateDto))]
[JsonSerializable(typeof(AuditFlowItemDto))]
[JsonSerializable(typeof(AuditFlowUpdateDto))]
[JsonSerializable(typeof(AuditRecordDetailDto))]
[JsonSerializable(typeof(AuditRecordItemDto))]
[JsonSerializable(typeof(AuditRecordSubmitDto))]
[JsonSerializable(typeof(AuditRecordWithdrawDto))]
[JsonSerializable(typeof(AuditDecisionDto))]
[JsonSerializable(typeof(AuditTaskDetailDto))]
[JsonSerializable(typeof(AuditTaskItemDto))]
[JsonSerializable(typeof(ArticleCategoryAddDto))]
[JsonSerializable(typeof(ArticleCategoryDetailDto))]
[JsonSerializable(typeof(ArticleCategoryItemDto))]
[JsonSerializable(typeof(ArticleCategoryUpdateDto))]
[JsonSerializable(typeof(ArticleAddDto))]
[JsonSerializable(typeof(ArticleDetailDto))]
[JsonSerializable(typeof(ArticleImageUploadDto))]
[JsonSerializable(typeof(ArticleItemDto))]
[JsonSerializable(typeof(ArticleUpdateDto))]
[JsonSerializable(typeof(Article))]
[JsonSerializable(typeof(ArticleCategory))]
[JsonSerializable(typeof(ResCategory))]
[JsonSerializable(typeof(ResDefinitionItemDto))]
[JsonSerializable(typeof(ResDefinitionPropertyItemDto))]
[JsonSerializable(typeof(ResEnvironment))]
[JsonSerializable(typeof(ResGroup))]
[JsonSerializable(typeof(ResPermission))]
[JsonSerializable(typeof(ResTag))]
[JsonSerializable(typeof(SysConfig))]
[JsonSerializable(typeof(SysDataScope))]
[JsonSerializable(typeof(SysDataScopeGroup))]
[JsonSerializable(typeof(SysMenu))]
[JsonSerializable(typeof(SysRole))]
[JsonSerializable(typeof(SysUser))]
[JsonSerializable(typeof(InAppNotificationAddDto))]
[JsonSerializable(typeof(InAppNotificationDetailDto))]
[JsonSerializable(typeof(InAppNotificationFilterDto))]
[JsonSerializable(typeof(InAppNotificationItemDto))]
[JsonSerializable(typeof(InAppNotificationReadDto))]
[JsonSerializable(typeof(InAppNotificationUpdateDto))]
[JsonSerializable(typeof(NotificationDeliveryItemDto))]
[JsonSerializable(typeof(NotificationEventItemDto))]
[JsonSerializable(typeof(NotificationReceiverAddDto))]
[JsonSerializable(typeof(NotificationReceiverItemDto))]
[JsonSerializable(typeof(NotificationReceiverUpdateDto))]
[JsonSerializable(typeof(NotificationRecoveryDto))]
[JsonSerializable(typeof(NotificationRecoveryResultDto))]
[JsonSerializable(typeof(NotificationRuleAddDto))]
[JsonSerializable(typeof(NotificationRuleItemDto))]
[JsonSerializable(typeof(NotificationRuleUpdateDto))]
[JsonSerializable(typeof(NotificationSourceAddDto))]
[JsonSerializable(typeof(NotificationSourceItemDto))]
[JsonSerializable(typeof(NotificationSourceUpdateDto))]
[JsonSerializable(typeof(WebhookResultDto))]
[JsonSerializable(typeof(NotificationWebhookDispatcher))]
[JsonSerializable(typeof(FilterBase))]
[JsonSerializable(typeof(PageList<AuditFlowItemDto>))]
[JsonSerializable(typeof(PageList<AuditRecordItemDto>))]
[JsonSerializable(typeof(PageList<AuditTaskItemDto>))]
[JsonSerializable(typeof(PageList<ArticleCategoryItemDto>))]
[JsonSerializable(typeof(PageList<ArticleItemDto>))]
[JsonSerializable(typeof(PageList<SysMenu>))]
[JsonSerializable(typeof(PageList<InAppNotificationItemDto>))]
[JsonSerializable(typeof(PageList<NotificationDeliveryItemDto>))]
[JsonSerializable(typeof(PageList<NotificationEventItemDto>))]
[JsonSerializable(typeof(PageList<NotificationReceiverItemDto>))]
[JsonSerializable(typeof(PageList<NotificationRuleItemDto>))]
[JsonSerializable(typeof(PageList<NotificationSourceItemDto>))]
[JsonSerializable(typeof(PageList<ResourceItemDto>))]
[JsonSerializable(typeof(PageList<UserFavoriteResourceItemDto>))]
[JsonSerializable(typeof(PageList<UserResourceItemDto>))]
[JsonSerializable(typeof(PageList<SysConfigItemDto>))]
[JsonSerializable(typeof(PageList<SysDataScopeItemDto>))]
[JsonSerializable(typeof(PageList<SysDataScopeGroupItemDto>))]
[JsonSerializable(typeof(PageList<SysLogsItemDto>))]
[JsonSerializable(typeof(PageList<SysRoleItemDto>))]
[JsonSerializable(typeof(PageList<SysUserItemDto>))]
[JsonSerializable(typeof(EnumDictionary))]
[JsonSerializable(typeof(ResCategoryAddDto))]
[JsonSerializable(typeof(ResCategoryUpdateDto))]
[JsonSerializable(typeof(ResDefinitionAddDto))]
[JsonSerializable(typeof(ResDefinitionUpdateDto))]
[JsonSerializable(typeof(ResDefinitionPropertyAddDto))]
[JsonSerializable(typeof(ResDefinitionPropertyUpdateDto))]
[JsonSerializable(typeof(ResEnvironmentAddDto))]
[JsonSerializable(typeof(ResEnvironmentUpdateDto))]
[JsonSerializable(typeof(ResGroupAddDto))]
[JsonSerializable(typeof(ResGroupUpdateDto))]
[JsonSerializable(typeof(ResPermissionUpdateDto))]
[JsonSerializable(typeof(ResTagAddDto))]
[JsonSerializable(typeof(ResTagUpdateDto))]
[JsonSerializable(typeof(ResourceAddDto))]
[JsonSerializable(typeof(ResourceCreatedDto))]
[JsonSerializable(typeof(ResourceDetailDto))]
[JsonSerializable(typeof(ResourceItemDto))]
[JsonSerializable(typeof(ResourceUpdateDto))]
[JsonSerializable(typeof(UserFavoriteResourceAddDto))]
[JsonSerializable(typeof(UserFavoriteResourceCreatedDto))]
[JsonSerializable(typeof(UserFavoriteResourceDetailDto))]
[JsonSerializable(typeof(UserFavoriteResourceItemDto))]
[JsonSerializable(typeof(UserResourceAddDto))]
[JsonSerializable(typeof(UserResourceCreatedDto))]
[JsonSerializable(typeof(UserResourceDetailDto))]
[JsonSerializable(typeof(UserResourceItemDto))]
[JsonSerializable(typeof(UserResourceRejectDto))]
[JsonSerializable(typeof(UserResourceReviewDto))]
[JsonSerializable(typeof(UserResourceUpdateDto))]
[JsonSerializable(typeof(Localizer))]
[JsonSerializable(typeof(AccessTokenDto))]
[JsonSerializable(typeof(ExternalUserInfoDto))]
[JsonSerializable(typeof(Dictionary<string, List<EnumDictionary>>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(List<ResCategory>))]
[JsonSerializable(typeof(List<ResDefinitionItemDto>))]
[JsonSerializable(typeof(List<ResDefinitionPropertyItemDto>))]
[JsonSerializable(typeof(List<ResEnvironment>))]
[JsonSerializable(typeof(List<ResGroup>))]
[JsonSerializable(typeof(List<ResPermission>))]
[JsonSerializable(typeof(List<ResTag>))]
[JsonSerializable(typeof(List<EnumDictionary>))]
[JsonSerializable(typeof(List<Guid>))]
[JsonSerializable(typeof(List<SysMenuSyncDto>))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(Guid?))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(SysConfigAddDto))]
[JsonSerializable(typeof(SysConfigDetailDto))]
[JsonSerializable(typeof(SysConfigFilterDto))]
[JsonSerializable(typeof(SysConfigItemDto))]
[JsonSerializable(typeof(SysConfigUpdateDto))]
[JsonSerializable(typeof(SysDataScopeAddDto))]
[JsonSerializable(typeof(SysDataScopeDetailDto))]
[JsonSerializable(typeof(SysDataScopeFilterDto))]
[JsonSerializable(typeof(SysDataScopeItemDto))]
[JsonSerializable(typeof(SysDataScopeUpdateDto))]
[JsonSerializable(typeof(SysDataScopeGroupAddDto))]
[JsonSerializable(typeof(SysDataScopeGroupDetailDto))]
[JsonSerializable(typeof(SysDataScopeGroupFilterDto))]
[JsonSerializable(typeof(SysDataScopeGroupItemDto))]
[JsonSerializable(typeof(SysDataScopeGroupUpdateDto))]
[JsonSerializable(typeof(SysLoginDto))]
[JsonSerializable(typeof(SysLogsFilterDto))]
[JsonSerializable(typeof(SysLogsItemDto))]
[JsonSerializable(typeof(SysMenuAddDto))]
[JsonSerializable(typeof(SysMenuFilterDto))]
[JsonSerializable(typeof(SysMenuSyncDto))]
[JsonSerializable(typeof(SysMenuUpdateDto))]
[JsonSerializable(typeof(SysOrganizationAddDto))]
[JsonSerializable(typeof(SysOrganizationItemDto))]
[JsonSerializable(typeof(SysOrganizationUpdateDto))]
[JsonSerializable(typeof(SysRoleAddDto))]
[JsonSerializable(typeof(SysRoleDetailDto))]
[JsonSerializable(typeof(SysRoleItemDto))]
[JsonSerializable(typeof(SysRoleSetMenusDto))]
[JsonSerializable(typeof(SysRoleUpdateDto))]
[JsonSerializable(typeof(SysUserDataScopeGroupSetUsersDto))]
[JsonSerializable(typeof(SysUserAddDto))]
[JsonSerializable(typeof(SysUserDetailDto))]
[JsonSerializable(typeof(SysUserItemDto))]
[JsonSerializable(typeof(SysUserOptionDto[]))]
[JsonSerializable(typeof(SysUserUpdateDto))]
[JsonSerializable(typeof(UserInfoDto))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(List<SysOrganizationItemDto>))]
// </perigon-aot-json:generated>
// OpenAPI describes multipart files and flattened query parameters via JSON metadata.
// These types are not JSON request bodies.
[JsonSerializable(typeof(Microsoft.AspNetCore.Http.IFormFile))]
[JsonSerializable(typeof(Entity.AuditMod.AuditRecordStatus?))]
[JsonSerializable(typeof(Entity.AuditMod.AuditTaskStatus?))]
[JsonSerializable(typeof(Entity.CMSMod.ContentType?))]
[JsonSerializable(typeof(Entity.CMSMod.LanguageType?))]
[JsonSerializable(typeof(Entity.ResourceMod.UserResourceStatus?))]
[JsonSerializable(typeof(Entity.ResourceMod.UserResourceAuditStatus?))]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(Guid?))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ErrorResult))]
public partial class ApiJsonSerializerContext : JsonSerializerContext
{
}

