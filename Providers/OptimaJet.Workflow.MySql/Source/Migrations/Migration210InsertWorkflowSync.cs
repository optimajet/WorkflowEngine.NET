using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(210)]
public class Migration210InsertWorkflowSync : Migration
{
    public override void Up()
    {
        this.EmbeddedScript("InsertIgnore_WorkflowSync.sql");
    }

    public override void Down()
    {
    }
}
