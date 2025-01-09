using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(210)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.InsertIgnore_WorkflowSync.sql")]
public class Migration210InsertWorkflowSync : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
