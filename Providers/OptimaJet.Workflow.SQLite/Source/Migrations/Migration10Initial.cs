using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

/// <summary>
/// Initial migration that creates the core workflow engine database schema.
/// </summary>
[Migration(10)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.InitialWorkflowEngineSchema.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration10Initial : Migration
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
