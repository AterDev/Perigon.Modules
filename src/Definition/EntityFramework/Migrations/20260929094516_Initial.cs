using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Level = table.Column<short>(type: "smallint", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleCategories_ArticleCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ArticleCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ResCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CatalogCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Icon = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResDefinitionProperties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MaxLength = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResDefinitionProperties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Icon = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResEnvironments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Icon = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResEnvironments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Icon = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Valid = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysDataScopeGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysDataScopeGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Path = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Icon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    AccessCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MenuType = table.Column<int>(type: "integer", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    Hidden = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysMenus_SysMenus_ParentId",
                        column: x => x.ParentId,
                        principalTable: "SysMenus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SysOrganizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysOrganizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysOrganizations_SysOrganizations_ParentId",
                        column: x => x.ParentId,
                        principalTable: "SysOrganizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SysRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NameValue = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    Icon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RealName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordSalt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    LastLoginTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastPwdEditTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Avatar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Sex = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DbConnectionString = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AnalysisConnectionString = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Domain = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Content = table.Column<string>(type: "character varying(200000)", maxLength: 200000, nullable: false),
                    Authors = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LanguageType = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    IsAudit = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsOriginal = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_ArticleCategories_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "ArticleCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResGroups_ResCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ResCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResDefinitionPropertyMaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResDefinitionPropertyMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResDefinitionPropertyMaps_ResDefinitionProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "ResDefinitionProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResDefinitionPropertyMaps_ResDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "ResDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AuditStatus = table.Column<int>(type: "integer", nullable: false),
                    ApprovedResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserResources_ResDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "ResDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResPermissions_ResCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ResCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResPermissions_ResEnvironments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "ResEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SysDataScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ResourceCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    ScopeType = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysDataScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysDataScopes_SysDataScopeGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SysDataScopeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SysMenuRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysMenuRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysMenuRoles_SysMenus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "SysMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysMenuRoles_SysRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SysRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SysLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Route = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SysUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysLogs_SysUsers_SysUserId",
                        column: x => x.SysUserId,
                        principalTable: "SysUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_SysUserDataScopeGroups_SysDataScopeGroups_DataScopeGroupId",
                        column: x => x.DataScopeGroupId,
                        principalTable: "SysDataScopeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysUserDataScopeGroups_SysUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SysUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SysUserOrganizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUserOrganizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysUserOrganizations_SysOrganizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "SysOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysUserOrganizations_SysUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SysUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SysUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysUserRoles_SysRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SysRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysUserRoles_SysUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SysUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagNames = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_ResCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ResCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resources_ResDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "ResDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resources_ResEnvironments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "ResEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resources_ResGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ResGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserResValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionPropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PropertyNameSnapshot = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ValueTypeSnapshot = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserResValues_ResDefinitionProperties_DefinitionPropertyId",
                        column: x => x.DefinitionPropertyId,
                        principalTable: "ResDefinitionProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserResValues_UserResources_UserResourceId",
                        column: x => x.UserResourceId,
                        principalTable: "UserResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionPropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PropertyNameSnapshot = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ValueTypeSnapshot = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResValues_ResDefinitionProperties_DefinitionPropertyId",
                        column: x => x.DefinitionPropertyId,
                        principalTable: "ResDefinitionProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResValues_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavoriteResources_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCategories_ParentId",
                table: "ArticleCategories",
                column: "ParentId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CatalogId",
                table: "Articles",
                column: "CatalogId");

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
                name: "IX_ResCategories_TenantId_CatalogCode",
                table: "ResCategories",
                columns: new[] { "TenantId", "CatalogCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionProperties_TenantId_Name",
                table: "ResDefinitionProperties",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_DefinitionId",
                table: "ResDefinitionPropertyMaps",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_PropertyId",
                table: "ResDefinitionPropertyMaps",
                column: "PropertyId");

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
                name: "IX_ResGroups_CategoryId",
                table: "ResGroups",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ResGroups_TenantId_CategoryId",
                table: "ResGroups",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CategoryId",
                table: "Resources",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_DefinitionId",
                table: "Resources",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_EnvironmentId",
                table: "Resources",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GroupId",
                table: "Resources",
                column: "GroupId");

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
                name: "IX_ResPermissions_CategoryId",
                table: "ResPermissions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ResPermissions_EnvironmentId",
                table: "ResPermissions",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResPermissions_TenantId_CategoryId",
                table: "ResPermissions",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ResPermissions_TenantId_EnvironmentId",
                table: "ResPermissions",
                columns: new[] { "TenantId", "EnvironmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ResPermissions_TenantId_RoleId_EnvironmentId_CategoryId",
                table: "ResPermissions",
                columns: new[] { "TenantId", "RoleId", "EnvironmentId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResValues_DefinitionPropertyId",
                table: "ResValues",
                column: "DefinitionPropertyId");

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
                name: "IX_SysConfigs_TenantId_GroupName_Key",
                table: "SysConfigs",
                columns: new[] { "TenantId", "GroupName", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysDataScopeGroups_TenantId_Name",
                table: "SysDataScopeGroups",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SysDataScopes_GroupId",
                table: "SysDataScopes",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SysDataScopes_TenantId_GroupId",
                table: "SysDataScopes",
                columns: new[] { "TenantId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysDataScopes_TenantId_Name",
                table: "SysDataScopes",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SysLogs_SysUserId",
                table: "SysLogs",
                column: "SysUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SysLogs_TenantId_ActionType_CreatedTime",
                table: "SysLogs",
                columns: new[] { "TenantId", "ActionType", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SysLogs_TenantId_ActionUserName_CreatedTime",
                table: "SysLogs",
                columns: new[] { "TenantId", "ActionUserName", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SysLogs_TenantId_CreatedTime",
                table: "SysLogs",
                columns: new[] { "TenantId", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SysLogs_TenantId_SysUserId",
                table: "SysLogs",
                columns: new[] { "TenantId", "SysUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysMenuRoles_MenuId",
                table: "SysMenuRoles",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_SysMenuRoles_RoleId",
                table: "SysMenuRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SysMenuRoles_TenantId_MenuId",
                table: "SysMenuRoles",
                columns: new[] { "TenantId", "MenuId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysMenuRoles_TenantId_RoleId_MenuId",
                table: "SysMenuRoles",
                columns: new[] { "TenantId", "RoleId", "MenuId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysMenus_ParentId",
                table: "SysMenus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_SysMenus_TenantId_AccessCode",
                table: "SysMenus",
                columns: new[] { "TenantId", "AccessCode" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysMenus_TenantId_ParentId",
                table: "SysMenus",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysOrganizations_ParentId",
                table: "SysOrganizations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_SysOrganizations_TenantId_Name",
                table: "SysOrganizations",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SysOrganizations_TenantId_ParentId",
                table: "SysOrganizations",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysRoles_TenantId_Name",
                table: "SysRoles",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SysRoles_TenantId_NameValue",
                table: "SysRoles",
                columns: new[] { "TenantId", "NameValue" },
                unique: true,
                filter: "\"IsDeleted\" = false");

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

            migrationBuilder.CreateIndex(
                name: "IX_SysUserOrganizations_OrganizationId",
                table: "SysUserOrganizations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SysUserOrganizations_TenantId_OrganizationId",
                table: "SysUserOrganizations",
                columns: new[] { "TenantId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysUserOrganizations_TenantId_UserId_OrganizationId",
                table: "SysUserOrganizations",
                columns: new[] { "TenantId", "UserId", "OrganizationId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysUserOrganizations_UserId",
                table: "SysUserOrganizations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SysUserRoles_RoleId",
                table: "SysUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SysUserRoles_TenantId_RoleId",
                table: "SysUserRoles",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysUserRoles_TenantId_UserId_RoleId",
                table: "SysUserRoles",
                columns: new[] { "TenantId", "UserId", "RoleId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysUserRoles_UserId",
                table: "SysUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SysUsers_TenantId_Email",
                table: "SysUsers",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysUsers_TenantId_PhoneNumber",
                table: "SysUsers",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Domain",
                table: "Tenants",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteResources_ResourceId",
                table: "UserFavoriteResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteResources_TenantId_ResourceId",
                table: "UserFavoriteResources",
                columns: new[] { "TenantId", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteResources_TenantId_UserId_ResourceId",
                table: "UserFavoriteResources",
                columns: new[] { "TenantId", "UserId", "ResourceId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserResources_DefinitionId",
                table: "UserResources",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResources_TenantId_AuditStatus",
                table: "UserResources",
                columns: new[] { "TenantId", "AuditStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_UserResources_TenantId_DefinitionId",
                table: "UserResources",
                columns: new[] { "TenantId", "DefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserResources_TenantId_UserId",
                table: "UserResources",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserResValues_DefinitionPropertyId",
                table: "UserResValues",
                column: "DefinitionPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResValues_TenantId_DefinitionPropertyId",
                table: "UserResValues",
                columns: new[] { "TenantId", "DefinitionPropertyId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserResValues_TenantId_UserResourceId_DefinitionPropertyId",
                table: "UserResValues",
                columns: new[] { "TenantId", "UserResourceId", "DefinitionPropertyId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserResValues_UserResourceId",
                table: "UserResValues",
                column: "UserResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "ResDefinitionPropertyMaps");

            migrationBuilder.DropTable(
                name: "ResPermissions");

            migrationBuilder.DropTable(
                name: "ResTags");

            migrationBuilder.DropTable(
                name: "ResValues");

            migrationBuilder.DropTable(
                name: "SysConfigs");

            migrationBuilder.DropTable(
                name: "SysDataScopes");

            migrationBuilder.DropTable(
                name: "SysLogs");

            migrationBuilder.DropTable(
                name: "SysMenuRoles");

            migrationBuilder.DropTable(
                name: "SysUserDataScopeGroups");

            migrationBuilder.DropTable(
                name: "SysUserOrganizations");

            migrationBuilder.DropTable(
                name: "SysUserRoles");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "UserFavoriteResources");

            migrationBuilder.DropTable(
                name: "UserResValues");

            migrationBuilder.DropTable(
                name: "ArticleCategories");

            migrationBuilder.DropTable(
                name: "SysMenus");

            migrationBuilder.DropTable(
                name: "SysDataScopeGroups");

            migrationBuilder.DropTable(
                name: "SysOrganizations");

            migrationBuilder.DropTable(
                name: "SysRoles");

            migrationBuilder.DropTable(
                name: "SysUsers");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "ResDefinitionProperties");

            migrationBuilder.DropTable(
                name: "UserResources");

            migrationBuilder.DropTable(
                name: "ResEnvironments");

            migrationBuilder.DropTable(
                name: "ResGroups");

            migrationBuilder.DropTable(
                name: "ResDefinitions");

            migrationBuilder.DropTable(
                name: "ResCategories");
        }
    }
}
