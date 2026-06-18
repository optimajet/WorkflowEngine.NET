using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(15)]
[WorkflowEngineMigration("OptimaJet.Workflow.MSSQL.Scripts.CreateCalendarNameColumn.sql")]
public class Migration15CreateCalendarNameColumn : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
