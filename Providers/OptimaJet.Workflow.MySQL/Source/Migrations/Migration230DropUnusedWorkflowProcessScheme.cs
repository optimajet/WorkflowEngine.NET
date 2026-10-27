using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(230)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateFunction_DropUnusedWorkflowProcessScheme.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration230DropUnusedWorkflowProcessScheme : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
