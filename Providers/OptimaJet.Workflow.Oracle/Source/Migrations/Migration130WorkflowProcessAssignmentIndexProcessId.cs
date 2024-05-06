using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(130)]
public class Migration130WorkflowProcessAssignmentIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_PROCESSID").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessAssignment_ProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
