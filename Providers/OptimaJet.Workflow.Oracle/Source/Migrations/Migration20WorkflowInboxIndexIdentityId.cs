using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(20)]
public class Migration20WorkflowInboxIndexIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WorkflowInbox").Index("IDX_WORKFLOWINBOX_IDENTITYID").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowInbox_IdentityId.sql");
        }
    }

    public override void Down()
    {
    }
}
