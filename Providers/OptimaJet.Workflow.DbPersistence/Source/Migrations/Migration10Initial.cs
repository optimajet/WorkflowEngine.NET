using FluentMigrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(10)]
public class Migration10Initial : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("OptimaJet.Workflow.MSSQL.Scripts.InitialWorkflowEngineSchema.sql");
    }

    public override void Down()
    {
    }
}
