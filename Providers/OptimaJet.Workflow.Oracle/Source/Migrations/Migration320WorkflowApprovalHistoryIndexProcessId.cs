using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(320)]
public class Migration320WorkflowApprovalHistoryIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWAPPROVALHISTORY").Index("IDX_WORKFLOWAPPROVALHISTORY_PID").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowApprovalHistory_ProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
