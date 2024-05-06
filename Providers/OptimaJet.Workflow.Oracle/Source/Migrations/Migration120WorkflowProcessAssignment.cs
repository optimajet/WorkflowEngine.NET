using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(120)]
public class Migration120WorkflowProcessAssignment : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowProcessAssignment.sql");
        }
    }

    public override void Down()
    {
    }
}
