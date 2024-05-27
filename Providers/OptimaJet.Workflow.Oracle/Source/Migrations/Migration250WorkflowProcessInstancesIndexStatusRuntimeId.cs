using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(250)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessInstances_StatusRuntimeId.sql")]
public class Migration250WorkflowProcessInstancesIndexStatusRuntimeId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCES").Index("IDX_WORKFLOWPROCESSINSTANCES_SR").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
