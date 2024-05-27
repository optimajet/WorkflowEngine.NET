using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(140)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessAssignment_AssignmentCode.sql")]
public class Migration140WorkflowProcessAssignmentIndexAssignmentCode : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_ASSIGNMENTCODE").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
