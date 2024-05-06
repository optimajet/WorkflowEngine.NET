using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(150)]
public class Migration150WorkflowProcessAssignmentIndexExecutor : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_EXECUTOR").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessAssignment_Executor.sql");
        }
    }

    public override void Down()
    {
    }
}
