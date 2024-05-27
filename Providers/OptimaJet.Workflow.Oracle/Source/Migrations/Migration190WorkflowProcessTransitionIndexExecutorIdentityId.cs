using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(190)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessTransition_ExecutorIdentityId.sql")]
public class Migration190WorkflowProcessTransitionIndexExecutorIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTRANSITIONH").Index("IDX_WORKFLOWPROCESSTRANSITIONH_EX").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
