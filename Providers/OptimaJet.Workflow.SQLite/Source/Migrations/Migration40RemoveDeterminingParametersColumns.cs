using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

[Migration(40)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.RemoveDeterminingParametersColumns.sql")]
public class Migration40RemoveDeterminingParametersColumns : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
        // Down migration not supported
    }
}
