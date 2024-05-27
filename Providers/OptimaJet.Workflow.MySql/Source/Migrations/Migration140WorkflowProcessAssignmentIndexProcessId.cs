using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(140)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessAssignment_ProcessIdExecutor.sql")]
public class Migration140WorkflowProcessAssignmentIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessassignment").Index("ix_workflowprocessassignment_processid_executor").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
