using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.PostgreSQL.Migrations;

[Migration(40)]
[WorkflowEngineMigration("OptimaJet.Workflow.PostgreSQL.Scripts.RemoveConstraintsFromDeterminingParametersColumns.sql")]
public class Migration40RemoveConstraintsFromDeterminingParameters : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
        // Down migration not supported - this is a deprecation step
        // Columns will be removed completely in the next release
    }
}
