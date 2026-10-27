using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(50)]
[WorkflowEngineMigration("OptimaJet.Workflow.MSSQL.Scripts.WorkflowTenantIdAndSchemeId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration50WorkflowTenantIdAndSchemeId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
