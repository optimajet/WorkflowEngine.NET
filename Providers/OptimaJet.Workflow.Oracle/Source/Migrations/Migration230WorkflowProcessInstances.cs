using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(230)]
public class Migration230WorkflowProcessInstances : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCES").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowProcessInstances.sql");
        }
    }

    public override void Down()
    {
    }
}
