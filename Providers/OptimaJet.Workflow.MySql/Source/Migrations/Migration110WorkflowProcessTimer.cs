using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(110)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateTable_WorkflowProcessTimer.sql")]
public class Migration110WorkflowProcessTimer : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
