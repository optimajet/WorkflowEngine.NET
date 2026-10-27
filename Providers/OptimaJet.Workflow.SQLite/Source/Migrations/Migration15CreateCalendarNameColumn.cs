using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

/// <summary>
/// Migration that adds the calendar name column to the workflow process instance table.
/// </summary>
[Migration(15)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.CreateCalendarNameColumn.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration15CreateCalendarNameColumn : Migration
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
