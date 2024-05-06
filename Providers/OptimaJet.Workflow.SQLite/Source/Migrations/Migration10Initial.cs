using FluentMigrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

[Migration(10)]
public class Migration10Initial : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("OptimaJet.Workflow.SQLite.Scripts.InitialWorkflowEngineSchema.sql");
    }

    public override void Down()
    {
    }
}
