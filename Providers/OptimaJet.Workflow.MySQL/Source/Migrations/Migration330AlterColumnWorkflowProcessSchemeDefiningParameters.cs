using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(330)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowProcessScheme_DefiningParameters.sql")]
[ProductVersion("WorkflowEngine", "21.0.0")]
public class Migration330AlterColumnWorkflowProcessSchemeDefiningParameters : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
