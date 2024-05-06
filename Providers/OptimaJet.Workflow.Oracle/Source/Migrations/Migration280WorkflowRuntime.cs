using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(280)]
public class Migration280WorkflowRuntime : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWRUNTIME").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowRuntime.sql");
        }
    }

    public override void Down()
    {
    }
}
