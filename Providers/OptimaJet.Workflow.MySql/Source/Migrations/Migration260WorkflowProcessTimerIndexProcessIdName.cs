using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(260)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessTimer_ProcessId_Name.sql")]
public class Migration260WorkflowProcessTimerIndexProcessIdName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocesstimer").Index("ix_workflowprocesstimer_processId_name").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
