using Perigon.PostgreSQL.Metadata;
using Perigon.PostgreSQL.Migrations;

namespace EntityFramework.AppDbContext;

public partial class DefaultDbContext
{
    // Perigon.PostgreSQL 0.2.1 loses explicit foreign keys when constructing its
    // migration model. Preserve the existing reviewed schema for design-time
    // migration generation and baseline comparison until that library is fixed.
    public new MigrationModel GetMigrationModel()
    {
        var model = base.GetMigrationModel();
        var foreignKeys = new Dictionary<string, MigrationForeignKey[]>(StringComparer.Ordinal)
        {
            ["ArticleCategories"] =
            [
            new("FK_ArticleCategories_ArticleCategories_ParentId", ["ParentId"], null, "ArticleCategories", ["Id"], ReferentialAction.NoAction)
            ],
            ["SysMenus"] =
            [
            new("FK_SysMenus_SysMenus_ParentId", ["ParentId"], null, "SysMenus", ["Id"], ReferentialAction.NoAction)
            ],
            ["SysOrganizations"] =
            [
            new("FK_SysOrganizations_SysOrganizations_ParentId", ["ParentId"], null, "SysOrganizations", ["Id"], ReferentialAction.NoAction)
            ],
            ["Articles"] =
            [
            new("FK_Articles_ArticleCategories_CatalogId", ["CatalogId"], null, "ArticleCategories", ["Id"], ReferentialAction.Cascade)
            ],
            ["ResGroups"] =
            [
            new("FK_ResGroups_ResCategories_CategoryId", ["CategoryId"], null, "ResCategories", ["Id"], ReferentialAction.Cascade)
            ],
            ["ResDefinitionPropertyMaps"] =
            [
            new("FK_ResDefinitionPropertyMaps_ResDefinitionProperties_PropertyId", ["PropertyId"], null, "ResDefinitionProperties", ["Id"], ReferentialAction.Cascade),
            new("FK_ResDefinitionPropertyMaps_ResDefinitions_DefinitionId", ["DefinitionId"], null, "ResDefinitions", ["Id"], ReferentialAction.Cascade)
            ],
            ["UserResources"] =
            [
            new("FK_UserResources_ResDefinitions_DefinitionId", ["DefinitionId"], null, "ResDefinitions", ["Id"], ReferentialAction.Restrict)
            ],
            ["ResPermissions"] =
            [
            new("FK_ResPermissions_ResCategories_CategoryId", ["CategoryId"], null, "ResCategories", ["Id"], ReferentialAction.Cascade),
            new("FK_ResPermissions_ResEnvironments_EnvironmentId", ["EnvironmentId"], null, "ResEnvironments", ["Id"], ReferentialAction.Cascade)
            ],
            ["SysDataScopes"] =
            [
            new("FK_SysDataScopes_SysDataScopeGroups_GroupId", ["GroupId"], null, "SysDataScopeGroups", ["Id"], ReferentialAction.Cascade)
            ],
            ["SysMenuRoles"] =
            [
            new("FK_SysMenuRoles_SysMenus_MenuId", ["MenuId"], null, "SysMenus", ["Id"], ReferentialAction.Cascade),
            new("FK_SysMenuRoles_SysRoles_RoleId", ["RoleId"], null, "SysRoles", ["Id"], ReferentialAction.Cascade)
            ],
            ["SysLogs"] =
            [
            new("FK_SysLogs_SysUsers_SysUserId", ["SysUserId"], null, "SysUsers", ["Id"], ReferentialAction.Cascade)
            ],
            ["SysUserDataScopeGroups"] =
            [
            new("FK_SysUserDataScopeGroups_SysDataScopeGroups_DataScopeGroupId", ["DataScopeGroupId"], null, "SysDataScopeGroups", ["Id"], ReferentialAction.Cascade),
            new("FK_SysUserDataScopeGroups_SysUsers_UserId", ["UserId"], null, "SysUsers", ["Id"], ReferentialAction.Cascade)
            ],
            ["SysUserOrganizations"] =
            [
            new("FK_SysUserOrganizations_SysOrganizations_OrganizationId", ["OrganizationId"], null, "SysOrganizations", ["Id"], ReferentialAction.Cascade),
            new("FK_SysUserOrganizations_SysUsers_UserId", ["UserId"], null, "SysUsers", ["Id"], ReferentialAction.Cascade)
            ],
            ["SysUserRoles"] =
            [
            new("FK_SysUserRoles_SysRoles_RoleId", ["RoleId"], null, "SysRoles", ["Id"], ReferentialAction.Cascade),
            new("FK_SysUserRoles_SysUsers_UserId", ["UserId"], null, "SysUsers", ["Id"], ReferentialAction.Cascade)
            ],
            ["Resources"] =
            [
            new("FK_Resources_ResCategories_CategoryId", ["CategoryId"], null, "ResCategories", ["Id"], ReferentialAction.Cascade),
            new("FK_Resources_ResDefinitions_DefinitionId", ["DefinitionId"], null, "ResDefinitions", ["Id"], ReferentialAction.Cascade),
            new("FK_Resources_ResEnvironments_EnvironmentId", ["EnvironmentId"], null, "ResEnvironments", ["Id"], ReferentialAction.Cascade),
            new("FK_Resources_ResGroups_GroupId", ["GroupId"], null, "ResGroups", ["Id"], ReferentialAction.NoAction)
            ],
            ["UserResValues"] =
            [
            new("FK_UserResValues_ResDefinitionProperties_DefinitionPropertyId", ["DefinitionPropertyId"], null, "ResDefinitionProperties", ["Id"], ReferentialAction.Restrict),
            new("FK_UserResValues_UserResources_UserResourceId", ["UserResourceId"], null, "UserResources", ["Id"], ReferentialAction.Cascade)
            ],
            ["ResValues"] =
            [
            new("FK_ResValues_ResDefinitionProperties_DefinitionPropertyId", ["DefinitionPropertyId"], null, "ResDefinitionProperties", ["Id"], ReferentialAction.Cascade),
            new("FK_ResValues_Resources_ResourceId", ["ResourceId"], null, "Resources", ["Id"], ReferentialAction.Cascade)
            ],
            ["UserFavoriteResources"] =
            [
            new("FK_UserFavoriteResources_Resources_ResourceId", ["ResourceId"], null, "Resources", ["Id"], ReferentialAction.Cascade)
            ],
        };
        return new MigrationModel(model.Tables.Select(table => new MigrationTable(
            table.Schema, table.Name, table.Comment, table.Columns,
            table.PrimaryKey is null ? null : new MigrationPrimaryKey("PK_" + table.Name, table.PrimaryKey.Columns),
            foreignKeys.TryGetValue(table.Name, out var keys) ? keys : [], table.Indexes)).ToArray());
    }
}
