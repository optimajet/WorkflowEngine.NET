using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(70)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessInstanceStatus_Status.sql")]
public class Migration70WorkflowProcessInstanceStatusIndexStatus : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstancestatus").Index("IX_WorkflowProcessInstanceStatus_Status").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
