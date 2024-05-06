using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(190)]
public class Migration190WorkflowProcessTransitionIndexExecutorIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTRANSITIONH").Index("IDX_WORKFLOWPROCESSTRANSITIONH_EX").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessTransition_ExecutorIdentityId.sql");
        }
    }

    public override void Down()
    {
    }
}
