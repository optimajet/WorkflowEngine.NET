using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(330)]
public class Migration330WorkflowApprovalHistoryIndexIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWAPPROVALHISTORY").Index("IDX_WORKFLOWAPPROVALHISTORY_IDENTITYID").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowApprovalHistory_IdentityId.sql");
        }
    }

    public override void Down()
    {
    }
}
