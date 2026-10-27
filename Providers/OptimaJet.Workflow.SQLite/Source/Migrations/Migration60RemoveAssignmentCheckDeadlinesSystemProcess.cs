using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

/// <summary>
/// Migration that removes the assignment check deadlines system process.
/// </summary>
[Migration(60)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.RemoveAssignmentCheckDeadlinesSystemProcess.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration60RemoveAssignmentCheckDeadlinesSystemProcess : Migration
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
