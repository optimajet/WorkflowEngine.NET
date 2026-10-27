using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

/// <summary>
/// Migration that removes the determining parameters columns from the workflow schema table.
/// </summary>
[Migration(40)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.RemoveDeterminingParametersColumns.sql")]
[ProductVersion("WorkflowEngine", "21.0.0")]
public class Migration40RemoveDeterminingParametersColumns : Migration
{
    /// <inheritdoc/>
    public override void Up()
    {
        this.EmbeddedScript();
    }

    /// <inheritdoc/>
    public override void Down()
    {
        // Down migration not supported
    }
}
