using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddResDefinitionPropertyReuse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResDefinitionProperties_ResDefinitions_DefinitionId",
                table: "ResDefinitionProperties");

            migrationBuilder.DropIndex(
                name: "IX_ResDefinitionProperties_DefinitionId_Name",
                table: "ResDefinitionProperties");

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

            migrationBuilder.AddColumn<string>(
                name: "NameKey",
                table: "ResDefinitionProperties",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE \"ResDefinitionProperties\" SET \"NameKey\" = lower(trim(\"Name\"));");

            // Preserve the existing one-to-many rows as definition-property mappings.
            // The old property Id is a safe mapping Id because each old property belonged
            // to exactly one definition.
            migrationBuilder.Sql(
                """
                INSERT INTO "ResDefinitionPropertyMaps"
                    ("Id", "DefinitionId", "PropertyId", "Sort", "CreatedTime", "UpdatedTime", "IsDeleted", "TenantId")
                SELECT "Id", "DefinitionId", "Id", "Sort", "CreatedTime", "UpdatedTime", "IsDeleted", "TenantId"
                FROM "ResDefinitionProperties";
                """);

            // Properties with the same name in one tenant become one reusable property.
            // Rewrite both mappings and historical values before removing the duplicates.
            migrationBuilder.Sql(
                """
                WITH ranked AS (
                    SELECT "Id",
                           FIRST_VALUE("Id") OVER (
                               PARTITION BY "TenantId", "NameKey"
                               ORDER BY "Id") AS "CanonicalId"
                    FROM "ResDefinitionProperties"
                )
                UPDATE "ResDefinitionPropertyMaps" AS map
                SET "PropertyId" = ranked."CanonicalId"
                FROM ranked
                WHERE map."PropertyId" = ranked."Id"
                  AND ranked."Id" <> ranked."CanonicalId";

                WITH ranked AS (
                    SELECT "Id",
                           FIRST_VALUE("Id") OVER (
                               PARTITION BY "TenantId", "NameKey"
                               ORDER BY "Id") AS "CanonicalId"
                    FROM "ResDefinitionProperties"
                )
                UPDATE "ResValues" AS value
                SET "DefinitionPropertyId" = ranked."CanonicalId"
                FROM ranked
                WHERE value."DefinitionPropertyId" = ranked."Id"
                  AND ranked."Id" <> ranked."CanonicalId";

                WITH ranked AS (
                    SELECT "Id",
                           FIRST_VALUE("Id") OVER (
                               PARTITION BY "TenantId", "NameKey"
                               ORDER BY "Id") AS "CanonicalId"
                    FROM "ResDefinitionProperties"
                )
                DELETE FROM "ResDefinitionProperties" AS property
                USING ranked
                WHERE property."Id" = ranked."Id"
                  AND ranked."Id" <> ranked."CanonicalId";
                """);

            migrationBuilder.DropColumn(
                name: "DefinitionId",
                table: "ResDefinitionProperties");

            migrationBuilder.DropColumn(
                name: "Sort",
                table: "ResDefinitionProperties");

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionProperties_TenantId_NameKey",
                table: "ResDefinitionProperties",
                columns: new[] { "TenantId", "NameKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_DefinitionId_PropertyId",
                table: "ResDefinitionPropertyMaps",
                columns: new[] { "DefinitionId", "PropertyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionPropertyMaps_PropertyId",
                table: "ResDefinitionPropertyMaps",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResDefinitionPropertyMaps");

            migrationBuilder.DropIndex(
                name: "IX_ResDefinitionProperties_TenantId_NameKey",
                table: "ResDefinitionProperties");

            migrationBuilder.DropColumn(
                name: "NameKey",
                table: "ResDefinitionProperties");

            migrationBuilder.AddColumn<Guid>(
                name: "DefinitionId",
                table: "ResDefinitionProperties",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Sort",
                table: "ResDefinitionProperties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionProperties_DefinitionId_Name",
                table: "ResDefinitionProperties",
                columns: new[] { "DefinitionId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ResDefinitionProperties_ResDefinitions_DefinitionId",
                table: "ResDefinitionProperties",
                column: "DefinitionId",
                principalTable: "ResDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
