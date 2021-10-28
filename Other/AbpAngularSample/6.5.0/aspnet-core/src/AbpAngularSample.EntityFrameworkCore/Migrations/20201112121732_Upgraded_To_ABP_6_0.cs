using Microsoft.EntityFrameworkCore.Migrations;

namespace AbpAngularSample.Migrations
{
    public partial class Upgraded_To_ABP_6_0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewValueHash",
                table: "AbpEntityPropertyChanges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalValueHash",
                table: "AbpEntityPropertyChanges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AbpDynamicProperties",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewValueHash",
                table: "AbpEntityPropertyChanges");

            migrationBuilder.DropColumn(
                name: "OriginalValueHash",
                table: "AbpEntityPropertyChanges");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AbpDynamicProperties");
        }
    }
}
