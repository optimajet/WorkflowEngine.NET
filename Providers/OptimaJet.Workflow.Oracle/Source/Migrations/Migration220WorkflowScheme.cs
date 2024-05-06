using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(220)]
public class Migration220WorkflowScheme : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWSCHEME").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowScheme.sql");
        }
    }

    public override void Down()
    {
    }
}
