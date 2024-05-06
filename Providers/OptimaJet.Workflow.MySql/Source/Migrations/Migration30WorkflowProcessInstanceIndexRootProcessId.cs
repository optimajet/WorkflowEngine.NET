using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(30)]
public class Migration30WorkflowProcessInstanceIndexRootProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstance").Index("ix_workflowprocessinstance_rootprocessid").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstance_RootProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
