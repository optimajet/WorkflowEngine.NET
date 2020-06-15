using Microsoft.EntityFrameworkCore.Migrations;

namespace AngularBPWorkflow.Migrations
{
    public partial class Added_Description_And_IsActive_To_Role : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AbpRoles",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AbpRoles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AbpRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AbpRoles");
        }
    }
}
