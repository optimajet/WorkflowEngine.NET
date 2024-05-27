using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(180)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessTransition_ProcessId.sql")]
public class Migration180WorkflowProcessTransitionIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTRANSITIONH").Index("IDX_WORKFLOWPROCESSTRANSITIONH").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
