using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(50)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateTable_WorkflowProcessInstancePersistence.sql")]
public class Migration50WorkflowProcessInstancePersistence : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
