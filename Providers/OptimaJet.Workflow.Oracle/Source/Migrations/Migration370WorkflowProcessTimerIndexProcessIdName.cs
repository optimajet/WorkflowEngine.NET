using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(370)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessTimer_ProcessId_Name.sql")]
public class Migration370WorkflowProcessTimerIndexProcessIdName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTIMER").Index("IDX_WORKFLOWPROCESSTIMER_PN").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
