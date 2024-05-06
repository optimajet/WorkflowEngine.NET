using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(40)]
public class Migration40WorkflowProcessInstanceIndexCalendarName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstance").Index("ix_calendarname").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstance_CalendarName.sql");
        }
    }

    public override void Down()
    {
    }
}
