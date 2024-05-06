using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(190)]
public class Migration190WorkflowRuntime : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowRuntime.sql");
    }

    public override void Down()
    {
    }
}
