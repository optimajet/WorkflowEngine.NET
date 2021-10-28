using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AbpAngularSample.Migrations
{
    public partial class Upgrade_ABP_383 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "AbpUserTokens",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "AbpUserTokens");
        }
    }
}
