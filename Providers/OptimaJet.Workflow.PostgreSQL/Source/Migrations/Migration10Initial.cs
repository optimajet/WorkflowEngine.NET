using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.PostgreSQL.Migrations;

[Migration(10)]
[WorkflowEngineMigration("OptimaJet.Workflow.PostgreSQL.Scripts.InitialWorkflowEngineSchema.sql")]
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
