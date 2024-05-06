using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(130)]
public class Migration130WorkflowProcessAssignment : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowProcessAssignment.sql");
    }

    public override void Down()
    {
    }
}
