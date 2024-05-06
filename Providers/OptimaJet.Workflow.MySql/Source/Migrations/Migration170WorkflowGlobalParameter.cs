using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(170)]
public class Migration170WorkflowGlobalParameter : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowGlobalParameter.sql");
    }

    public override void Down()
    {
    }
}
