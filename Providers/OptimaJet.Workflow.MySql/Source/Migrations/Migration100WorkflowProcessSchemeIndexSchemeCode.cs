using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(100)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessScheme_SchemeCode.sql")]
public class Migration100WorkflowProcessSchemeIndexSchemeCode : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessscheme").Index("ix_workflowprocessscheme_schemecode_hash_isobsolete").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
