using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(210)]
public class Migration210WorkflowProcessInstancePIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCEP").Index("IDX_WORKFLOWPROCESSINSTANCEP_P").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstanceP_ProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
