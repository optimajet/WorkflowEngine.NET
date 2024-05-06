using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(110)]
public class Migration110WorkflowProcessTimer : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowProcessTimer.sql");
    }

    public override void Down()
    {
    }
}
