using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(40)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessInstance_CalendarName.sql")]
public class Migration40WorkflowProcessInstanceIndexCalendarName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstance").Index("ix_calendarname").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
