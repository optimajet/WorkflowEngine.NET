using Microsoft.EntityFrameworkCore.Migrations;

namespace AngularBPWorkflow.Migrations
{
    public partial class Upgraded_To_Abp_2_1_0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpRoleClaims_AbpRoles_UserId",
                table: "AbpRoleClaims");

            migrationBuilder.DropIndex(
                name: "IX_AbpRoleClaims_UserId",
                table: "AbpRoleClaims");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AbpRoleClaims");

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "AbpLanguages",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_AbpRoleClaims_AbpRoles_RoleId",
                table: "AbpRoleClaims",
                column: "RoleId",
                principalTable: "AbpRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpRoleClaims_AbpRoles_RoleId",
                table: "AbpRoleClaims");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "AbpLanguages");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "AbpRoleClaims",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpRoleClaims_UserId",
                table: "AbpRoleClaims",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpRoleClaims_AbpRoles_UserId",
                table: "AbpRoleClaims",
                column: "UserId",
                principalTable: "AbpRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
