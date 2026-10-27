using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

/// <summary>
/// Migration that creates unique indexes for the workflow engine tables.
/// </summary>
[Migration(20)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.CreateUniqueIndexes.sql")]
[ProductVersion("WorkflowEngine", "14.0.0")]
public class Migration20CreateUniqueIndexes : Migration
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
