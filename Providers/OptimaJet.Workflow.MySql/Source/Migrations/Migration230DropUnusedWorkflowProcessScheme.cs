using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(230)]
public class Migration230DropUnusedWorkflowProcessScheme : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateFunction_DropUnusedWorkflowProcessScheme.sql");
    }

    public override void Down()
    {
    }
}
