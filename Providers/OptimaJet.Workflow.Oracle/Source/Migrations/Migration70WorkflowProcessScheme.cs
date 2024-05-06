using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(70)]
public class Migration70WorkflowProcessScheme : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSSCHEME").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowProcessScheme.sql");
        }
    }

    public override void Down()
    {
    }
}
