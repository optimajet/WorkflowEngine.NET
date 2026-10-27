using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(10)]
[WorkflowEngineMigration("OptimaJet.Workflow.MSSQL.Scripts.InitialWorkflowEngineSchema.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration10Initial : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
