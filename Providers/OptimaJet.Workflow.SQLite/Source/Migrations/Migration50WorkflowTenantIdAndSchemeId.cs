using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.SQLite.Migrations;

/// <summary>
/// Migration that adds tenant identifier and scheme identifier columns to workflow tables.
/// </summary>
[Migration(50)]
[WorkflowEngineMigration("OptimaJet.Workflow.SQLite.Scripts.WorkflowTenantIdAndSchemeId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration50WorkflowTenantIdAndSchemeId : Migration
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
