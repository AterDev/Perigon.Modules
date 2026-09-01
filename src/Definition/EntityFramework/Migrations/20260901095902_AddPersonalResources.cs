using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AuditStatus = table.Column<int>(type: "integer", nullable: false),
                    ValuesJson = table.Column<string>(type: "character varying(100000)", maxLength: 100000, nullable: false),
                    ApprovedResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalResources_ResDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "ResDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalResources_DefinitionId",
                table: "PersonalResources",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalResources_TenantId_AuditStatus",
                table: "PersonalResources",
                columns: new[] { "TenantId", "AuditStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalResources_TenantId_DefinitionId",
                table: "PersonalResources",
                columns: new[] { "TenantId", "DefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalResources_TenantId_UserId",
                table: "PersonalResources",
                columns: new[] { "TenantId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalResources");
        }
    }
}
