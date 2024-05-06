using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(340)]
public class Migration340DropUnusedWorkflowProcessScheme : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateFunction_DropUnusedWorkflowProcessScheme.sql");
    }

    public override void Down()
    {
    }
}
