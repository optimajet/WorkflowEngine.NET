using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

[Migration(30)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.CreateWorkflowForms.sql")]
public class Migration30CreateUniqueIndexes : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
