using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(20)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowInbox_IdentityId.sql")]
public class Migration20WorkflowInboxIndexIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WorkflowInbox").Index("IDX_WORKFLOWINBOX_IDENTITYID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
