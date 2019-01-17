using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AngularBPWorkflow.Migrations
{
    public partial class Upgraded_To_Abp_v3_6_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "AbpUserLoginAttempts",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "AbpEntityChangeSets",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "AbpAuditLogs",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "AbpUserLoginAttempts",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "AbpEntityChangeSets",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "AbpAuditLogs",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
