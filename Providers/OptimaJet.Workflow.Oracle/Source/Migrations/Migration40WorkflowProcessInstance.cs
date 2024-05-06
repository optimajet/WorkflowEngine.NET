using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(40)]
public class Migration40WorkflowProcessInstance : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCE").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowProcessInstance.sql");
        }
    }

    public override void Down()
    {
    }
}
