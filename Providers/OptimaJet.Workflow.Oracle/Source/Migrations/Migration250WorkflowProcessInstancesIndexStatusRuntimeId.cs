using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(250)]
public class Migration250WorkflowProcessInstancesIndexStatusRuntimeId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCES").Index("IDX_WORKFLOWPROCESSINSTANCES_SR").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstances_StatusRuntimeId.sql");
        }
    }

    public override void Down()
    {
    }
}
