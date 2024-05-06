using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(90)]
public class Migration90WorkflowProcessScheme : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowProcessScheme.sql");
    }

    public override void Down()
    {
    }
}
