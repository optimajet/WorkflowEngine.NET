using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(20)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateTable_WorkflowProcessInstance.sql")]
public class Migration20WorkflowProcessInstance : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
