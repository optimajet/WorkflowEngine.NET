using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(220)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowScheme.sql")]
public class Migration220WorkflowScheme : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWSCHEME").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
