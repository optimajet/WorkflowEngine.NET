using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(330)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowApprovalHistory_IdentityId.sql")]
public class Migration330WorkflowApprovalHistoryIndexIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWAPPROVALHISTORY").Index("IDX_WORKFLOWAPPROVALHISTORY_IDENTITYID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
