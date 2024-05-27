using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(300)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.Insert_WorkflowSync.sql")]
public class Migration300WorkflowSyncInsert : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
