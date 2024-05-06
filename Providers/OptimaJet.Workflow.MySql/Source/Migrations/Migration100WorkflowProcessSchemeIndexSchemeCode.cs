using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(100)]
public class Migration100WorkflowProcessSchemeIndexSchemeCode : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessscheme").Index("ix_workflowprocessscheme_schemecode_hash_isobsolete").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessScheme_SchemeCode.sql");
        }
    }

    public override void Down()
    {
    }
}
