using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(160)]
public class Migration160WorkflowScheme : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowScheme.sql");
    }

    public override void Down()
    {
    }
}
