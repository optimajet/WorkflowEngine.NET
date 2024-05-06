using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(50)]
public class Migration50WorkflowProcessInstancePersistence : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("CreateTable_WorkflowProcessInstancePersistence.sql");
    }

    public override void Down()
    {
    }
}
