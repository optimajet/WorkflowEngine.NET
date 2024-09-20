using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(20)]
[WorkflowEngineMigration("OptimaJet.Workflow.MSSQL.Scripts.CreateUniqueIndexes.sql")]
public class Migration20CreateUniqueIndexes : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
