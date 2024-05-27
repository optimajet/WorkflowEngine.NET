using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(30)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessInstance_RootProcessId.sql")]
public class Migration30WorkflowProcessInstanceIndexRootProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstance").Index("ix_workflowprocessinstance_rootprocessid").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
