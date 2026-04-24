using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(330)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowProcessScheme_DefiningParameters.sql")]
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
