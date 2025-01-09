using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(80)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessInstanceStatus_StatusRuntimeId.sql")]
public class Migration80WorkflowProcessInstanceStatusIndexRuntimeId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstancestatus").Index("IX_WorkflowProcessInstanceStatus_Status_Runtime").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
