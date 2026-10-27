using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(340)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateFunction_DropUnusedWorkflowProcessScheme.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration340DropUnusedWorkflowProcessScheme : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
