using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemoveResDefinitionPropertyNameKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResDefinitionProperties_TenantId_NameKey",
                table: "ResDefinitionProperties");

            migrationBuilder.DropColumn(
                name: "NameKey",
                table: "ResDefinitionProperties");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_ResDefinitionProperties_TenantId_Name_CaseInsensitive\" " +
                "ON \"ResDefinitionProperties\" (\"TenantId\", (lower(trim(\"Name\"))));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"IX_ResDefinitionProperties_TenantId_Name_CaseInsensitive\";");

            migrationBuilder.AddColumn<string>(
                name: "NameKey",
                table: "ResDefinitionProperties",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ResDefinitionProperties_TenantId_NameKey",
                table: "ResDefinitionProperties",
                columns: new[] { "TenantId", "NameKey" },
                unique: true);
        }
    }
}
