using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(2)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateFunction_UUIDToBin.sql")]
public class Migration2UUIDToBin : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
