using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(30)]
public class Migration30WorkflowInboxIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WorkflowInbox").Index("IDX_WORKFLOWINBOX_PROCESSID").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowInbox_ProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
