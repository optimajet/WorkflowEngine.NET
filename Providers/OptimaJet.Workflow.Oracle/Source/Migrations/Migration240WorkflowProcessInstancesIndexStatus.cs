using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(240)]
public class Migration240WorkflowProcessInstancesIndexStatus : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCES").Index("IDX_WORKFLOWPROCESSINSTANCES_S").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstances_Status.sql");
        }
    }

    public override void Down()
    {
    }
}
