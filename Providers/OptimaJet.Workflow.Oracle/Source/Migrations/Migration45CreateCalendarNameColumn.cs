using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(45)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateCalendarNameColumn.sql")]
public class Migration450CreateCalendarNameColumn : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
