using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(150)]
public class Migration150WorkflowProcessTransitionHistory : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowProcessTransitionHistory.sql");
    }

    public override void Down()
    {
    }
}
