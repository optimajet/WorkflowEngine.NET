using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(10)]
public class Migration10WorkflowInbox : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowInbox.sql");
    }

    public override void Down()
    {
    }
}
