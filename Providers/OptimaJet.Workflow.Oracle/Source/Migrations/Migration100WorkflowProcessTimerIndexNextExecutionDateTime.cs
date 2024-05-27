using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(100)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessTimer_NextExecutionDateTime.sql")]
public class Migration100WorkflowProcessTimerIndexNextExecutionDateTime : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTIMER").Index("IDX_WORKFLOWPROCESSTIMER_DATE").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
