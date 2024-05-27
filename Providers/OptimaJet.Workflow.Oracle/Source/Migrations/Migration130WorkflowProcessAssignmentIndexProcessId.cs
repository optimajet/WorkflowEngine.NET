using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(130)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessAssignment_ProcessId.sql")]
public class Migration130WorkflowProcessAssignmentIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_PROCESSID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
