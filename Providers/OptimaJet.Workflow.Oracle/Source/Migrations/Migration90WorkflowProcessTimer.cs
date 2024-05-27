using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(90)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowProcessTimer.sql")]
public class Migration90WorkflowProcessTimer : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTIMER").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
