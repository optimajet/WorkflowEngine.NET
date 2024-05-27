using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(190)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateTable_WorkflowRuntime.sql")]
public class Migration190WorkflowRuntime : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
