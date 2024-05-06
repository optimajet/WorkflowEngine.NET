using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(290)]
public class Migration290WorkflowSync : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWSYNC").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowSync.sql");
        }
    }

    public override void Down()
    {
    }
}
