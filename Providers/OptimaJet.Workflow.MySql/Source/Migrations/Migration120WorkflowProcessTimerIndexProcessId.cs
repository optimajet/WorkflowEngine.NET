using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(120)]
public class Migration120WorkflowProcessTimerIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocesstimer").Index("ix_workflowprocesstimer_processId").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessTimer_ProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
