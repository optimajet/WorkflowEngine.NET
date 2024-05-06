using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(160)]
public class Migration160WorkflowProcessAssignmentIndexProcessIdExecutor : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_PE").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessAssignment_ProcessIdExecutor.sql");
        }
    }

    public override void Down()
    {
    }
}
