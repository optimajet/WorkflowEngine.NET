using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(280)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowRuntime.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration280WorkflowRuntime : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWRUNTIME").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
