using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

[Migration(10)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.InitialWorkflowEngineSchema.sql")]
public class Migration10Initial : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
