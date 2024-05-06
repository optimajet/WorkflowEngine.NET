using FluentMigrator;

namespace OptimaJet.Workflow.PostgreSQL.Migrations;

[Migration(10)]
public class Migration10Initial : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("OptimaJet.Workflow.PostgreSQL.Scripts.InitialWorkflowEngineSchema.sql");
    }

    public override void Down()
    {
    }
}
