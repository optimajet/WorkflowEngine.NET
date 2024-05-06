using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(200)]
public class Migration200WorkflowSync : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowSync.sql");
    }

    public override void Down()
    {
    }
}
