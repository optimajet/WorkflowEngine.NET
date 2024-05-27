using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(10)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateTable_WorkflowInbox.sql")]
public class Migration10WorkflowInbox : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
