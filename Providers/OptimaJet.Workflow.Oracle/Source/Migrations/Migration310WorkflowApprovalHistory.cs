using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(310)]
public class Migration310WorkflowApprovalHistory : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWAPPROVALHISTORY").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowApprovalHistory.sql");
        }
    }

    public override void Down()
    {
    }
}
