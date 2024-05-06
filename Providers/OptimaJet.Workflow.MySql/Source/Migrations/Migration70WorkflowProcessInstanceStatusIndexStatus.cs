using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(70)]
public class Migration70WorkflowProcessInstanceStatusIndexStatus : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstancestatus").Index("IX_WorkflowProcessInstanceStatus_Status").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstanceStatus_Status.sql");
        }
    }

    public override void Down()
    {
    }
}
