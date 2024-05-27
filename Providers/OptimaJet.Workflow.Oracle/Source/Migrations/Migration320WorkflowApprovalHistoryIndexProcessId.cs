using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(320)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowApprovalHistory_ProcessId.sql")]
public class Migration320WorkflowApprovalHistoryIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWAPPROVALHISTORY").Index("IDX_WORKFLOWAPPROVALHISTORY_PID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
