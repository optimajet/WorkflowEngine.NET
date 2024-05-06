using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(60)]
public class Migration60WorkflowProcessInstanceStatus : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowProcessInstanceStatus.sql");
    }

    public override void Down()
    {
    }
}
