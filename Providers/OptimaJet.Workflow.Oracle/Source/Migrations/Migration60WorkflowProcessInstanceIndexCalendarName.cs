using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(60)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessInstance_CalendarName.sql")]
public class Migration60WorkflowProcessInstanceIndexCalendarName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCE").Index("IDX_CALENDARNAME").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
