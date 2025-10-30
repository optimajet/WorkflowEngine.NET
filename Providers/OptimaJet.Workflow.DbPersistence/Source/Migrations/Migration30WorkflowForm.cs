using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(30)]
[WorkflowEngineMigration("OptimaJet.Workflow.MSSQL.Scripts.CreateWorkflowForms.sql")]
public class Migration30WorkflowForm : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
