using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(260)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowGlobalParameter.sql")]
public class Migration260WorkflowGlobalParameter : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWGLOBALPARAMETER").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
