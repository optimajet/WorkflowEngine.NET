using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(20)]
public class Migration20WorkflowProcessInstance : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowProcessInstance.sql");
    }

    public override void Down()
    {
    }
}
