using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(50)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessInstance_RootProcessId.sql")]
public class Migration50WorkflowProcessInstanceIndexRootProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCE").Index("IDX_WORKFLOWPROCESSINSTANCE_ROOT").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
