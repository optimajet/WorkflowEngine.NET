using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(170)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowProcessTransition.sql")]
public class Migration170WorkflowProcessTransition : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTRANSITIONH").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
