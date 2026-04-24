using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(320)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowProcessScheme_DefiningParametersHash.sql")]
public class Migration320AlterColumnWorkflowProcessSchemeDefiningParametersHash : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
