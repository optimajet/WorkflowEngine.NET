using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AbpAngularSample.Migrations
{
    public partial class Upgrade_To_ABP_6_1_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey("PK_AbpDynamicPropertyValues", "AbpDynamicPropertyValues");
            migrationBuilder.DropColumn("Id", "AbpDynamicPropertyValues");
            migrationBuilder.AddColumn<long>(
                    name: "Id",
                    table: "AbpDynamicPropertyValues",
                    nullable: false)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
            migrationBuilder.AddPrimaryKey(
                name: "PK_AbpDynamicPropertyValues",
                table: "AbpDynamicPropertyValues",
                columns: new[] { "Id" });

            migrationBuilder.DropPrimaryKey("PK_AbpDynamicEntityPropertyValues", "AbpDynamicEntityPropertyValues");
            migrationBuilder.DropColumn("Id", "AbpDynamicEntityPropertyValues");
            migrationBuilder.AddColumn<long>(
                    name: "Id",
                    table: "AbpDynamicEntityPropertyValues",
                    nullable: false)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AbpDynamicEntityPropertyValues",
                table: "AbpDynamicEntityPropertyValues",
                columns: new[] { "Id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey("PK_AbpDynamicPropertyValues", "AbpDynamicPropertyValues");
            migrationBuilder.DropColumn("Id", "AbpDynamicPropertyValues");
            migrationBuilder.AddColumn<int>(
                    name: "Id",
                    table: "AbpDynamicPropertyValues",
                    nullable: false)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
            migrationBuilder.AddPrimaryKey(
                name: "PK_AbpDynamicPropertyValues",
                table: "AbpDynamicPropertyValues",
                columns: new[] { "Id" });

            migrationBuilder.DropPrimaryKey("PK_AbpDynamicEntityPropertyValues", "AbpDynamicEntityPropertyValues");
            migrationBuilder.DropColumn("Id", "AbpDynamicEntityPropertyValues");
            migrationBuilder.AddColumn<int>(
                    name: "Id",
                    table: "AbpDynamicEntityPropertyValues",
                    nullable: false)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
            migrationBuilder.AddPrimaryKey(
                name: "PK_AbpDynamicEntityPropertyValues",
                table: "AbpDynamicEntityPropertyValues",
                columns: new[] { "Id" });
        }
    }
}
