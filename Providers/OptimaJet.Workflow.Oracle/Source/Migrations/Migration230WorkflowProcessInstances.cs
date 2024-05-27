using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(230)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowProcessInstances.sql")]
public class Migration230WorkflowProcessInstances : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCES").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
