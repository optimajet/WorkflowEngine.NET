using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(140)]
public class Migration140WorkflowProcessAssignmentIndexAssignmentCode : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_ASSIGNMENTCODE").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessAssignment_AssignmentCode.sql");
        }
    }

    public override void Down()
    {
    }
}
