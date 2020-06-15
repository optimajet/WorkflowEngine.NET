using Microsoft.EntityFrameworkCore.Migrations;

namespace AngularBPWorkflow.Migrations
{
    public partial class Upgraded_To_Abp_v222 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AbpUserOrganizationUnits",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AbpUserOrganizationUnits");
        }
    }
}
