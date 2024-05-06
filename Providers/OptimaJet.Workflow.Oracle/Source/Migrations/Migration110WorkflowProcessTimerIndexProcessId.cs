using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(110)]
public class Migration110WorkflowProcessTimerIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSTIMER").Index("IX_PROCESSID").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessTimer_ProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
