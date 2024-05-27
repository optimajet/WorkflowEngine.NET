using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(30)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowInbox_ProcessId.sql")]
public class Migration30WorkflowInboxIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WorkflowInbox").Index("IDX_WORKFLOWINBOX_PROCESSID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
