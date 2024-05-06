using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(90)]
public class Migration90WorkflowProcessTimer : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTIMER").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowProcessTimer.sql");
        }
    }

    public override void Down()
    {
    }
}
