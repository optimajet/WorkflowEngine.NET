using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.PostgreSQL.Migrations;

[Migration(30)]
[WorkflowEngineMigration("OptimaJet.Workflow.PostgreSQL.Scripts.CreateWorkflowForms.sql")]
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
