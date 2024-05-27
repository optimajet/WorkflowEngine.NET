using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(310)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowApprovalHistory.sql")]
public class Migration310WorkflowApprovalHistory : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWAPPROVALHISTORY").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
