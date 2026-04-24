using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(440)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AlterColumn_WorkflowProcessScheme_DefiningParameters.sql")]
public class Migration440AlterColumnWorkflowProcessSchemeDefiningParameters : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
