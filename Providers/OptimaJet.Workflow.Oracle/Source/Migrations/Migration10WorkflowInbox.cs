using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(10)]
public class Migration10WorkflowInbox : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WorkflowInbox").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowInbox.sql");
        }
    }

    public override void Down()
    {
    }
}
