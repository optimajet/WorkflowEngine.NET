using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(80)]
public class Migration80WorkflowProcessInstanceStatusIndexRuntimeId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstancestatus").Index("IX_WorkflowProcessInstanceStatus_Status_Runtime").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstanceStatus_StatusRuntimeId.sql");
        }
    }

    public override void Down()
    {
    }
}
