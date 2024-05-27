using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(160)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessAssignment_ProcessIdExecutor.sql")]
public class Migration160WorkflowProcessAssignmentIndexProcessIdExecutor : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_PE").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
