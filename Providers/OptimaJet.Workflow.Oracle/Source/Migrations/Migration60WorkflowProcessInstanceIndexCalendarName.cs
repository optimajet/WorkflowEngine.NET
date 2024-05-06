using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(60)]
public class Migration60WorkflowProcessInstanceIndexCalendarName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCE").Index("IDX_CALENDARNAME").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstance_CalendarName.sql");
        }
    }

    public override void Down()
    {
    }
}
