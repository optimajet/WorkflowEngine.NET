using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(150)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessAssignment_Executor.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration150WorkflowProcessAssignmentIndexExecutor : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Index("IDX_WORKFLOWPROCESSASSIGNMENT_EXECUTOR").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
