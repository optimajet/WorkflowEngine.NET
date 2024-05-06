using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(180)]
public class Migration180WorkflowProcessTransitionIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTRANSITIONH").Index("IDX_WORKFLOWPROCESSTRANSITIONH").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessTransition_ProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
