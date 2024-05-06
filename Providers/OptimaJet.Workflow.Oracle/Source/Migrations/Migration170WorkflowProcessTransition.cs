using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(170)]
public class Migration170WorkflowProcessTransition : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTRANSITIONH").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowProcessTransition.sql");
        }
    }

    public override void Down()
    {
    }
}
