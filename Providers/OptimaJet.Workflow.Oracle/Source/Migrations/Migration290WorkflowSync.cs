using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(290)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowSync.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration290WorkflowSync : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWSYNC").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
