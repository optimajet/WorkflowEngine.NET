using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(430)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AlterColumn_WorkflowProcessScheme_DefiningParametersHash.sql")]
public class Migration430AlterColumnWorkflowProcessSchemeDefiningParametersHash : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
