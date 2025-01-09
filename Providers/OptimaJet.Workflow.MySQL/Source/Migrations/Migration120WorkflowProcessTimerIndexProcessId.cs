using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(120)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessTimer_ProcessId.sql")]
public class Migration120WorkflowProcessTimerIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocesstimer").Index("ix_workflowprocesstimer_processId").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
