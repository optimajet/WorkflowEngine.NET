using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(220)]
public class Migration220WorkflowApprovalHistory : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowApprovalHistory.sql");
    }

    public override void Down()
    {
    }
}
