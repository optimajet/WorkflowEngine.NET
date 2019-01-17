using Microsoft.EntityFrameworkCore.Migrations;

namespace AngularBPWorkflow.Migrations
{
    public partial class Upgrade_ABP_381 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AbpUserTokens",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AbpUserTokens",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
