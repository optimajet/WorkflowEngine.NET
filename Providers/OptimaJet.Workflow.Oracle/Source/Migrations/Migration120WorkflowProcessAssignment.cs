using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(120)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowProcessAssignment.sql")]
public class Migration120WorkflowProcessAssignment : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSASSIGNMENT").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
