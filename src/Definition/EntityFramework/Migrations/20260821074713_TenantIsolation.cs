using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class TenantIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemUsers_Email",
                table: "SystemUsers");

            migrationBuilder.DropIndex(
                name: "IX_SystemUsers_PhoneNumber",
                table: "SystemUsers");

            migrationBuilder.DropIndex(
                name: "IX_SystemUserRoles_UserId_RoleId",
                table: "SystemUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemRoles_Name",
                table: "SystemRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemRoles_NameValue",
                table: "SystemRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemPermissions_Name",
                table: "SystemPermissions");

            migrationBuilder.DropIndex(
                name: "IX_SystemPermissionGroups_Name",
                table: "SystemPermissionGroups");

            migrationBuilder.DropIndex(
                name: "IX_SystemOrganizations_Name",
                table: "SystemOrganizations");

            migrationBuilder.DropIndex(
                name: "IX_SystemMenus_AccessCode",
                table: "SystemMenus");

            migrationBuilder.DropIndex(
                name: "IX_SystemMenuRoles_RoleId_MenuId",
                table: "SystemMenuRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_ActionType_CreatedTime",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_ActionUserName_CreatedTime",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_CreatedTime",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemConfigs_GroupName_Key",
                table: "SystemConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ResValues_ResourceId_DefinitionPropertyId",
                table: "ResValues");

            migrationBuilder.DropIndex(
                name: "IX_ResDefinitionPropertyMaps_DefinitionId_PropertyId",
                table: "ResDefinitionPropertyMaps");

            migrationBuilder.DropIndex(
                name: "IX_Articles_UserId_Title",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_ArticleCategories_UserId_Name",
                table: "ArticleCategories");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_TenantId_Email",
                table: "SystemUsers",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_TenantId_PhoneNumber",
                table: "SystemUsers",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRoles_TenantId_RoleId",
                table: "SystemUserRoles",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRoles_TenantId_UserId_RoleId",
                table: "SystemUserRoles",
                columns: new[] { "TenantId", "UserId", "RoleId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRoles_UserId",
                table: "SystemUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoles_TenantId_Name",
                table: "SystemRoles",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoles_TenantId_NameValue",
                table: "SystemRoles",
                columns: new[] { "TenantId", "NameValue" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_TenantId_GroupId",
                table: "SystemPermissions",
                columns: new[] { "TenantId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_TenantId_Name",
                table: "SystemPermissions",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissionGroups_TenantId_Name",
                table: "SystemPermissionGroups",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemOrganizations_TenantId_Name",
                table: "SystemOrganizations",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemOrganizations_TenantId_ParentId",
                table: "SystemOrganizations",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMenus_TenantId_AccessCode",
                table: "SystemMenus",
                columns: new[] { "TenantId", "AccessCode" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMenus_TenantId_ParentId",
                table: "SystemMenus",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMenuRoles_TenantId_RoleId_MenuId",
                table: "SystemMenuRoles",
                columns: new[] { "TenantId", "RoleId", "MenuId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMenuRoles_TenantId_SystemMenuId",
                table: "SystemMenuRoles",
                columns: new[] { "TenantId", "SystemMenuId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMenuRoles_TenantId_SystemRoleId",
                table: "SystemMenuRoles",
                columns: new[] { "TenantId", "SystemRoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_TenantId_ActionType_CreatedTime",
                table: "SystemLogs",
                columns: new[] { "TenantId", "ActionType", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_TenantId_ActionUserName_CreatedTime",
                table: "SystemLogs",
                columns: new[] { "TenantId", "ActionUserName", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_TenantId_CreatedTime",
                table: "SystemLogs",
                columns: new[] { "TenantId", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_TenantId_SystemUserId",
                table: "SystemLogs",
                columns: new[] { "TenantId", "SystemUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigs_TenantId_GroupName_Key",
                table: "SystemConfigs",
                columns: new[] { "TenantId", "GroupName", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ResValues_ResourceId",
                table: "ResValues",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResValues_TenantId_DefinitionPropertyId",
                table: "ResValues",
                columns: new[] { "TenantId", "DefinitionPropertyId" });

            migrationBuilder.CreateIndex(
                name: "IX_ResValues_TenantId_ResourceId_DefinitionPropertyId",
                table: "ResValues",
                columns: new[] { "TenantId", "ResourceId", "DefinitionPropertyId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ResPermissions_TenantId_CategoryId",
                table: "ResPermissions",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ResPermissions_TenantId_EnvironmentId",
                table: "ResPermissions",
                columns: new[] { "TenantId", "EnvironmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_TenantId_CategoryId",
                table: "Resources",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_TenantId_DefinitionId",
                table: "Resources",
                columns: new[] { "TenantId", "DefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_TenantId_EnvironmentId",
                table: "Resources",
                columns: new[] { "TenantId", "EnvironmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_TenantId_GroupId",
                table: "Resources",
                columns: new[] { "TenantId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_ResGroups_TenantId_CategoryId",
                table: "ResGroups",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_DefinitionId",
                table: "ResDefinitionPropertyMaps",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_TenantId_DefinitionId_PropertyId",
                table: "ResDefinitionPropertyMaps",
                columns: new[] { "TenantId", "DefinitionId", "PropertyId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_TenantId_PropertyId",
                table: "ResDefinitionPropertyMaps",
                columns: new[] { "TenantId", "PropertyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_TenantId_CatalogId",
                table: "Articles",
                columns: new[] { "TenantId", "CatalogId" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_TenantId_UserId_Title",
                table: "Articles",
                columns: new[] { "TenantId", "UserId", "Title" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCategories_TenantId_ParentId",
                table: "ArticleCategories",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCategories_TenantId_UserId_Name",
                table: "ArticleCategories",
                columns: new[] { "TenantId", "UserId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemUsers_TenantId_Email",
                table: "SystemUsers");

            migrationBuilder.DropIndex(
                name: "IX_SystemUsers_TenantId_PhoneNumber",
                table: "SystemUsers");

            migrationBuilder.DropIndex(
                name: "IX_SystemUserRoles_TenantId_RoleId",
                table: "SystemUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemUserRoles_TenantId_UserId_RoleId",
                table: "SystemUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemUserRoles_UserId",
                table: "SystemUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemRoles_TenantId_Name",
                table: "SystemRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemRoles_TenantId_NameValue",
                table: "SystemRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemPermissions_TenantId_GroupId",
                table: "SystemPermissions");

            migrationBuilder.DropIndex(
                name: "IX_SystemPermissions_TenantId_Name",
                table: "SystemPermissions");

            migrationBuilder.DropIndex(
                name: "IX_SystemPermissionGroups_TenantId_Name",
                table: "SystemPermissionGroups");

            migrationBuilder.DropIndex(
                name: "IX_SystemOrganizations_TenantId_Name",
                table: "SystemOrganizations");

            migrationBuilder.DropIndex(
                name: "IX_SystemOrganizations_TenantId_ParentId",
                table: "SystemOrganizations");

            migrationBuilder.DropIndex(
                name: "IX_SystemMenus_TenantId_AccessCode",
                table: "SystemMenus");

            migrationBuilder.DropIndex(
                name: "IX_SystemMenus_TenantId_ParentId",
                table: "SystemMenus");

            migrationBuilder.DropIndex(
                name: "IX_SystemMenuRoles_TenantId_RoleId_MenuId",
                table: "SystemMenuRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemMenuRoles_TenantId_SystemMenuId",
                table: "SystemMenuRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemMenuRoles_TenantId_SystemRoleId",
                table: "SystemMenuRoles");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_TenantId_ActionType_CreatedTime",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_TenantId_ActionUserName_CreatedTime",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_TenantId_CreatedTime",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_TenantId_SystemUserId",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemConfigs_TenantId_GroupName_Key",
                table: "SystemConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ResValues_ResourceId",
                table: "ResValues");

            migrationBuilder.DropIndex(
                name: "IX_ResValues_TenantId_DefinitionPropertyId",
                table: "ResValues");

            migrationBuilder.DropIndex(
                name: "IX_ResValues_TenantId_ResourceId_DefinitionPropertyId",
                table: "ResValues");

            migrationBuilder.DropIndex(
                name: "IX_ResPermissions_TenantId_CategoryId",
                table: "ResPermissions");

            migrationBuilder.DropIndex(
                name: "IX_ResPermissions_TenantId_EnvironmentId",
                table: "ResPermissions");

            migrationBuilder.DropIndex(
                name: "IX_Resources_TenantId_CategoryId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_TenantId_DefinitionId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_TenantId_EnvironmentId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_TenantId_GroupId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_ResGroups_TenantId_CategoryId",
                table: "ResGroups");

            migrationBuilder.DropIndex(
                name: "IX_ResDefinitionPropertyMaps_DefinitionId",
                table: "ResDefinitionPropertyMaps");

            migrationBuilder.DropIndex(
                name: "IX_ResDefinitionPropertyMaps_TenantId_DefinitionId_PropertyId",
                table: "ResDefinitionPropertyMaps");

            migrationBuilder.DropIndex(
                name: "IX_ResDefinitionPropertyMaps_TenantId_PropertyId",
                table: "ResDefinitionPropertyMaps");

            migrationBuilder.DropIndex(
                name: "IX_Articles_TenantId_CatalogId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_TenantId_UserId_Title",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_ArticleCategories_TenantId_ParentId",
                table: "ArticleCategories");

            migrationBuilder.DropIndex(
                name: "IX_ArticleCategories_TenantId_UserId_Name",
                table: "ArticleCategories");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_Email",
                table: "SystemUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_PhoneNumber",
                table: "SystemUsers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRoles_UserId_RoleId",
                table: "SystemUserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoles_Name",
                table: "SystemRoles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoles_NameValue",
                table: "SystemRoles",
                column: "NameValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_Name",
                table: "SystemPermissions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissionGroups_Name",
                table: "SystemPermissionGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SystemOrganizations_Name",
                table: "SystemOrganizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMenus_AccessCode",
                table: "SystemMenus",
                column: "AccessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemMenuRoles_RoleId_MenuId",
                table: "SystemMenuRoles",
                columns: new[] { "RoleId", "MenuId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_ActionType_CreatedTime",
                table: "SystemLogs",
                columns: new[] { "ActionType", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_ActionUserName_CreatedTime",
                table: "SystemLogs",
                columns: new[] { "ActionUserName", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_CreatedTime",
                table: "SystemLogs",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigs_GroupName_Key",
                table: "SystemConfigs",
                columns: new[] { "GroupName", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResValues_ResourceId_DefinitionPropertyId",
                table: "ResValues",
                columns: new[] { "ResourceId", "DefinitionPropertyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_DefinitionId_PropertyId",
                table: "ResDefinitionPropertyMaps",
                columns: new[] { "DefinitionId", "PropertyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_UserId_Title",
                table: "Articles",
                columns: new[] { "UserId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCategories_UserId_Name",
                table: "ArticleCategories",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }
    }
}
