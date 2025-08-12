using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(1)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateFunction_BinToUUID.sql")]
public class Migration1BinToUUID : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
