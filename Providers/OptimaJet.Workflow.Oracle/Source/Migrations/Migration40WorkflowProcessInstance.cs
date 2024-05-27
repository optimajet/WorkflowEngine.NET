using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(40)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowProcessInstance.sql")]
public class Migration40WorkflowProcessInstance : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCE").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
