using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(200)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateTable_WorkflowProcessInstanceP.sql")]
public class Migration200WorkflowProcessInstanceP : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCEP").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
