using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(110)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessTimer_ProcessId.sql")]
public class Migration110WorkflowProcessTimerIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTIMER").Index("IX_PROCESSID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
