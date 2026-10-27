using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(420)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessScheme_SchemeCodeIsObsolete.sql")]
[ProductVersion("WorkflowEngine", "21.0.0")]
public class Migration420CreateIndexWorkflowProcessSchemeSchemeCodeIsObsolete : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
