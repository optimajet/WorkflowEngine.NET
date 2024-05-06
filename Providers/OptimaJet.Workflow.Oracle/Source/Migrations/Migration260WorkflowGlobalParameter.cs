using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(260)]
public class Migration260WorkflowGlobalParameter : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWGLOBALPARAMETER").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowGlobalParameter.sql");
        }
    }

    public override void Down()
    {
    }
}
