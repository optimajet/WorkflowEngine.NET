using Microsoft.EntityFrameworkCore.Migrations;

namespace AbpAngularSample.Migrations
{
    public partial class Upgraded_To_Abp_4_7_0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpSettings_TenantId_Name",
                table: "AbpSettings");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettings_TenantId_Name_UserId",
                table: "AbpSettings",
                columns: new[] { "TenantId", "Name", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpSettings_TenantId_Name_UserId",
                table: "AbpSettings");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettings_TenantId_Name",
                table: "AbpSettings",
                columns: new[] { "TenantId", "Name" });
        }
    }
}
