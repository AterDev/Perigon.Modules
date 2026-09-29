using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddSysDataScopeManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_SystemUsers_SystemUserId",
                table: "SystemLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemMenuRoles_SystemMenus_SystemMenuId",
                table: "SystemMenuRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemMenuRoles_SystemRoles_SystemRoleId",
                table: "SystemMenuRoles");

            migrationBuilder.DropTable(
                name: "SystemPermissionGroupSystemRole");

            // Legacy permissions cannot be mapped to data scopes because they have no resource code or target IDs.
            migrationBuilder.Sql("DELETE FROM \"SystemPermissions\";");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SystemPermissions");

            migrationBuilder.DropColumn(
                name: "Enable",
                table: "SystemPermissions");

            migrationBuilder.RenameColumn(
                name: "PermissionType",
                table: "SystemPermissions",
                newName: "ScopeType");

            migrationBuilder.RenameColumn(
                name: "SystemRoleId",
                table: "SystemMenuRoles",
                newName: "SysRoleId");

            migrationBuilder.RenameColumn(
                name: "SystemMenuId",
                table: "SystemMenuRoles",
                newName: "SysMenuId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_TenantId_SystemRoleId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_TenantId_SysRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_TenantId_SystemMenuId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_TenantId_SysMenuId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_SystemRoleId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_SysRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_SystemMenuId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_SysMenuId");

            migrationBuilder.RenameColumn(
                name: "SystemUserId",
                table: "SystemLogs",
                newName: "SysUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLogs_TenantId_SystemUserId",
                table: "SystemLogs",
                newName: "IX_SystemLogs_TenantId_SysUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLogs_SystemUserId",
                table: "SystemLogs",
                newName: "IX_SystemLogs_SysUserId");

            migrationBuilder.AddColumn<string>(
                name: "ResourceCode",
                table: "SystemPermissions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "TargetIds",
                table: "SystemPermissions",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::uuid[]");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "SystemPermissionGroups",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "SysUserDataScopeGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataScopeGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUserDataScopeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysUserDataScopeGroups_SystemPermissionGroups_DataScopeGrou~",
                        column: x => x.DataScopeGroupId,
                        principalTable: "SystemPermissionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysUserDataScopeGroups_SystemUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysUserDataScopeGroups_DataScopeGroupId",
                table: "SysUserDataScopeGroups",
                column: "DataScopeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SysUserDataScopeGroups_TenantId_DataScopeGroupId",
                table: "SysUserDataScopeGroups",
                columns: new[] { "TenantId", "DataScopeGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysUserDataScopeGroups_TenantId_UserId_DataScopeGroupId",
                table: "SysUserDataScopeGroups",
                columns: new[] { "TenantId", "UserId", "DataScopeGroupId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysUserDataScopeGroups_UserId",
                table: "SysUserDataScopeGroups",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_SystemUsers_SysUserId",
                table: "SystemLogs",
                column: "SysUserId",
                principalTable: "SystemUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemMenuRoles_SystemMenus_SysMenuId",
                table: "SystemMenuRoles",
                column: "SysMenuId",
                principalTable: "SystemMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemMenuRoles_SystemRoles_SysRoleId",
                table: "SystemMenuRoles",
                column: "SysRoleId",
                principalTable: "SystemRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_SystemUsers_SysUserId",
                table: "SystemLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemMenuRoles_SystemMenus_SysMenuId",
                table: "SystemMenuRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemMenuRoles_SystemRoles_SysRoleId",
                table: "SystemMenuRoles");

            migrationBuilder.DropTable(
                name: "SysUserDataScopeGroups");

            migrationBuilder.DropColumn(
                name: "ResourceCode",
                table: "SystemPermissions");

            migrationBuilder.DropColumn(
                name: "TargetIds",
                table: "SystemPermissions");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "SystemPermissionGroups");

            migrationBuilder.RenameColumn(
                name: "ScopeType",
                table: "SystemPermissions",
                newName: "PermissionType");

            migrationBuilder.RenameColumn(
                name: "SysRoleId",
                table: "SystemMenuRoles",
                newName: "SystemRoleId");

            migrationBuilder.RenameColumn(
                name: "SysMenuId",
                table: "SystemMenuRoles",
                newName: "SystemMenuId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_TenantId_SysRoleId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_TenantId_SystemRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_TenantId_SysMenuId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_TenantId_SystemMenuId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_SysRoleId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_SystemRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMenuRoles_SysMenuId",
                table: "SystemMenuRoles",
                newName: "IX_SystemMenuRoles_SystemMenuId");

            migrationBuilder.RenameColumn(
                name: "SysUserId",
                table: "SystemLogs",
                newName: "SystemUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLogs_TenantId_SysUserId",
                table: "SystemLogs",
                newName: "IX_SystemLogs_TenantId_SystemUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLogs_SysUserId",
                table: "SystemLogs",
                newName: "IX_SystemLogs_SystemUserId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SystemPermissions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "SystemPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SystemPermissionGroupSystemRole",
                columns: table => new
                {
                    PermissionGroupsId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPermissionGroupSystemRole", x => new { x.PermissionGroupsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_SystemPermissionGroupSystemRole_SystemPermissionGroups_Perm~",
                        column: x => x.PermissionGroupsId,
                        principalTable: "SystemPermissionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemPermissionGroupSystemRole_SystemRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "SystemRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissionGroupSystemRole_RolesId",
                table: "SystemPermissionGroupSystemRole",
                column: "RolesId");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_SystemUsers_SystemUserId",
                table: "SystemLogs",
                column: "SystemUserId",
                principalTable: "SystemUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemMenuRoles_SystemMenus_SystemMenuId",
                table: "SystemMenuRoles",
                column: "SystemMenuId",
                principalTable: "SystemMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemMenuRoles_SystemRoles_SystemRoleId",
                table: "SystemMenuRoles",
                column: "SystemRoleId",
                principalTable: "SystemRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
