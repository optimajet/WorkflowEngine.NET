using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(10)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowInbox.sql")]
public class Migration10WorkflowInbox : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WorkflowInbox").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
