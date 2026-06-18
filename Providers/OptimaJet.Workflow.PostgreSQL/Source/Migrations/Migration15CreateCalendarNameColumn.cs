using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.PostgreSQL.Migrations;

[Migration(15)]
[WorkflowEngineMigration("OptimaJet.Workflow.PostgreSQL.Scripts.CreateCalendarNameColumn.sql")]
public class Migration50CreateCalendarNameColumn : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
