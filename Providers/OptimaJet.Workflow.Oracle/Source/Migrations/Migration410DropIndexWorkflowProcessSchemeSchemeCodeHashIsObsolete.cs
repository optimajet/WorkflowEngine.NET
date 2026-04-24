using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(410)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.DropIndex_WorkflowProcessScheme_SchemeCodeHashIsObsolete.sql")]
public class Migration410DropIndexWorkflowProcessSchemeSchemeCodeHashIsObsolete : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
