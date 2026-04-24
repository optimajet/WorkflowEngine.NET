using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(310)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessScheme_SchemeCodeIsObsolete.sql")]
public class Migration310CreateIndexWorkflowProcessSchemeSchemeCodeIsObsolete : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessscheme").Index("ix_workflowprocessscheme_schemecode_isobsolete").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
