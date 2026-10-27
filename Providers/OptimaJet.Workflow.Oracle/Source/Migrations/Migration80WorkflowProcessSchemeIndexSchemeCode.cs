using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(80)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessScheme_SchemeCode.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration80WorkflowProcessSchemeIndexSchemeCode : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSSCHEME").Index("IDX_WORKFLOWPROCESSSCHEME_SCHE").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
