using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(140)]
public class Migration140WorkflowProcessAssignmentIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessassignment").Index("ix_workflowprocessassignment_processid_executor").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessAssignment_ProcessIdExecutor.sql");
        }
    }

    public override void Down()
    {
    }
}
