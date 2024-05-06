using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(100)]
public class Migration100WorkflowProcessTimerIndexNextExecutionDateTime : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTIMER").Index("IDX_WORKFLOWPROCESSTIMER_DATE").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessTimer_NextExecutionDateTime.sql");
        }
    }

    public override void Down()
    {
    }
}
