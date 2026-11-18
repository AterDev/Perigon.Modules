using System.Reflection;
using CMSMod.Managers;
using Entity.CMSMod;
using Entity.SystemMod;
using Perigon.AspNetCore.Models;
using SystemMod.Managers;
using SystemMod.Models.SysConfigDtos;
using SystemMod.Models.SysLogsDtos;
using SystemMod.Models.SysUserDtos;

namespace ApiTest;

/// <summary>
/// Regression coverage for the explicit entity-to-DTO mappings used by the
/// system and CMS managers. These tests intentionally invoke the pure mapping
/// functions without starting PostgreSQL or Aspire.
/// </summary>
public sealed class ExplicitMappingRegressionTests
{
    [Test]
    public async Task SysUserMappings_ShouldCopyAllFieldsAndPreserveNulls()
    {
        Guid id = Guid.CreateVersion7();
        DateTimeOffset created = DateTimeOffset.Parse("2026-01-02T03:04:05+00:00");
        DateTimeOffset updated = DateTimeOffset.Parse("2026-02-03T04:05:06+00:00");
        DateTimeOffset lastLogin = DateTimeOffset.Parse("2026-03-04T05:06:07+00:00");
        var entity = new SysUser
        {
            Id = id,
            UserName = "mapping-user",
            RealName = null,
            Email = "mapping@example.test",
            EmailConfirmed = true,
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = true,
            LockoutEnd = null,
            LockoutEnabled = true,
            AccessFailedCount = 3,
            LastLoginTime = lastLogin,
            RetryCount = 2,
            Avatar = null,
            Sex = Sex.Female,
            CreatedTime = created,
            UpdatedTime = updated,
        };

        SysUserItemDto item = InvokeMap<SysUserItemDto>(typeof(SysUserManager), "MapItem", entity);
        SysUserDetailDto detail = InvokeMap<SysUserDetailDto>(typeof(SysUserManager), "MapDetail", entity);

        await Assert.That(item.Id).IsEqualTo(id);
        await Assert.That(item.UserName).IsEqualTo("mapping-user");
        await Assert.That(item.RealName).IsNull();
        await Assert.That(item.Email).IsEqualTo("mapping@example.test");
        await Assert.That(item.LastLoginTime).IsEqualTo(lastLogin);
        await Assert.That(item.Sex).IsEqualTo(GenderType.Female);
        await Assert.That(item.CreatedTime).IsEqualTo(created);

        await Assert.That(detail.Id).IsEqualTo(id);
        await Assert.That(detail.UserName).IsEqualTo("mapping-user");
        await Assert.That(detail.RealName).IsNull();
        await Assert.That(detail.Email).IsEqualTo("mapping@example.test");
        await Assert.That(detail.EmailConfirmed).IsTrue();
        await Assert.That(detail.PhoneNumber).IsNull();
        await Assert.That(detail.PhoneNumberConfirmed).IsFalse();
        await Assert.That(detail.TwoFactorEnabled).IsTrue();
        await Assert.That(detail.LockoutEnd).IsNull();
        await Assert.That(detail.LockoutEnabled).IsTrue();
        await Assert.That(detail.AccessFailedCount).IsEqualTo(3);
        await Assert.That(detail.LastLoginTime).IsEqualTo(lastLogin);
        await Assert.That(detail.RetryCount).IsEqualTo(2);
        await Assert.That(detail.Avatar).IsNull();
        await Assert.That(detail.Sex).IsEqualTo(GenderType.Female);
        await Assert.That(detail.CreatedTime).IsEqualTo(created);
        await Assert.That(detail.UpdatedTime).IsEqualTo(updated);
    }

    [Test]
    public async Task SysLogMapping_ShouldCopyEnumAndOptionalFieldsWithoutInventingValues()
    {
        Guid id = Guid.CreateVersion7();
        DateTimeOffset created = DateTimeOffset.Parse("2026-04-05T06:07:08+00:00");
        var entity = new SysLogs
        {
            Id = id,
            ActionUserName = "operator",
            TargetName = null,
            Route = "/api/example",
            ActionType = UserActionType.Update,
            Description = null,
            CreatedTime = created,
        };

        SysLogsItemDto item = InvokeMap<SysLogsItemDto>(typeof(SysLogsManager), "MapItem", entity);

        await Assert.That(item.Id).IsEqualTo(id);
        await Assert.That(item.ActionUserName).IsEqualTo("operator");
        await Assert.That(item.TargetName).IsNull();
        await Assert.That(item.Route).IsEqualTo("/api/example");
        await Assert.That(item.ActionType).IsEqualTo(UserActionType.Update);
        await Assert.That(item.Description).IsNull();
        await Assert.That(item.CreatedTime).IsEqualTo(created);
    }

    [Test]
    public async Task SysConfigMappings_ShouldCopyListAndDetailFieldsWithNullDescription()
    {
        Guid id = Guid.CreateVersion7();
        var entity = new SysConfig
        {
            Id = id,
            Key = "feature.enabled",
            Value = "true",
            Description = null,
            Valid = false,
            IsSystem = true,
            GroupName = string.Empty,
        };

        SysConfigItemDto item = InvokeMap<SysConfigItemDto>(typeof(SysConfigManager), "MapItem", entity);
        SysConfigDetailDto detail = InvokeMap<SysConfigDetailDto>(typeof(SysConfigManager), "MapDetail", entity);

        await Assert.That(item.Id).IsEqualTo(id);
        await Assert.That(item.Key).IsEqualTo("feature.enabled");
        await Assert.That(item.Value).IsEqualTo("true");
        await Assert.That(item.Description).IsNull();
        await Assert.That(item.Valid).IsFalse();
        await Assert.That(item.IsSystem).IsTrue();
        await Assert.That(item.GroupName).IsEqualTo(string.Empty);

        await Assert.That(detail.Key).IsEqualTo("feature.enabled");
        await Assert.That(detail.Description).IsNull();
        await Assert.That(detail.Valid).IsFalse();
        await Assert.That(detail.IsSystem).IsTrue();
        await Assert.That(detail.GroupName).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ArticleItemMapping_ShouldCopyAllReturnedFieldsAndKeepOptionalTitleNull()
    {
        Guid id = Guid.CreateVersion7();
        Guid userId = Guid.CreateVersion7();
        Guid catalogId = Guid.CreateVersion7();
        DateTimeOffset created = DateTimeOffset.Parse("2026-05-06T07:08:09+00:00");
        DateTimeOffset updated = DateTimeOffset.Parse("2026-06-07T08:09:10+00:00");
        var entity = new Article
        {
            Id = id,
            Title = "Mapped title",
            Description = null,
            Authors = "Mapped author",
            LanguageType = LanguageType.EN,
            ContentType = ContentType.Knowledge,
            IsAudit = true,
            IsPublic = false,
            IsOriginal = true,
            UserId = userId,
            CatalogId = catalogId,
            ViewCount = 42,
            CreatedTime = created,
            UpdatedTime = updated,
        };

        var item = InvokeMap<CMSMod.Models.ArticleDtos.ArticleItemDto>(typeof(ArticleManager), "MapItem", entity);

        await Assert.That(item.Id).IsEqualTo(id);
        await Assert.That(item.Title).IsEqualTo("Mapped title");
        await Assert.That(item.Description).IsNull();
        await Assert.That(item.Authors).IsEqualTo("Mapped author");
        await Assert.That(item.TranslateTitle).IsNull();
        await Assert.That(item.LanguageType).IsEqualTo(LanguageType.EN);
        await Assert.That(item.BlogType).IsEqualTo(ContentType.Knowledge);
        await Assert.That(item.IsAudit).IsTrue();
        await Assert.That(item.IsPublic).IsFalse();
        await Assert.That(item.IsOriginal).IsTrue();
        await Assert.That(item.UserId).IsEqualTo(userId);
        await Assert.That(item.CatalogId).IsEqualTo(catalogId);
        await Assert.That(item.ViewCount).IsEqualTo(42);
        await Assert.That(item.CreatedTime).IsEqualTo(created);
        await Assert.That(item.UpdatedTime).IsEqualTo(updated);
    }

    private static TDto InvokeMap<TDto>(Type managerType, string methodName, object entity)
    {
        MethodInfo method = managerType.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic,
                binder: null,
                types: [entity.GetType()],
                modifiers: null)
            ?? throw new InvalidOperationException($"Mapping method {managerType.Name}.{methodName} was not found.");

        return (TDto)(method.Invoke(null, [entity])
            ?? throw new InvalidOperationException($"Mapping method {managerType.Name}.{methodName} returned null."));
    }
}
