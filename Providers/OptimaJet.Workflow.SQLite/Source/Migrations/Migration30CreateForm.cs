using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

/// <summary>
/// Migration that creates the workflow forms table.
/// </summary>
[Migration(30)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.CreateWorkflowForms.sql")]
[ProductVersion("WorkflowEngine", "20.0.0")]
public class Migration30CreateUniqueIndexes : Migration
{
    /// <inheritdoc/>
    public override void Up()
    {
        this.EmbeddedScript();
    }

    /// <inheritdoc/>
    public override void Down()
    {
    }
}
