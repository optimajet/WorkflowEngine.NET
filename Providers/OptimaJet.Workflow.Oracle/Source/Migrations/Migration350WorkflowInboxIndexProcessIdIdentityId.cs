using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(350)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowInbox_ProcessId_IdentityId.sql")]
[ProductVersion("WorkflowEngine", "14.0.0")]
public class Migration350WorkflowInboxIndexProcessIdIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWINBOX").Index("IDX_WORKFLOWINBOX_PI").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
