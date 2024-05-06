using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(300)]
public class Migration300WorkflowSyncInsert : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("Insert_WorkflowSync.sql");
    }

    public override void Down()
    {
    }
}
