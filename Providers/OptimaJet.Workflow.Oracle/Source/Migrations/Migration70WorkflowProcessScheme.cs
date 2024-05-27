using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(70)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowProcessScheme.sql")]
public class Migration70WorkflowProcessScheme : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSSCHEME").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
