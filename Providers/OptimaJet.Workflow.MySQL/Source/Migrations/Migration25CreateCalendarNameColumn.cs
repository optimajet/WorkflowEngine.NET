using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(25)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowProcessInstance_CalendarName.sql")]
public class Migration25CreateCalendarNameColumn : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
