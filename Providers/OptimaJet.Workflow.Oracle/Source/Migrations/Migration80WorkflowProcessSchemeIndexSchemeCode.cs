using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(80)]
public class Migration80WorkflowProcessSchemeIndexSchemeCode : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSSCHEME").Index("IDX_WORKFLOWPROCESSSCHEME_SCHE").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessScheme_SchemeCode.sql");
        }
    }

    public override void Down()
    {
    }
}
