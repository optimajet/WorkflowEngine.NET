using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(240)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessInstances_Status.sql")]
public class Migration240WorkflowProcessInstancesIndexStatus : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCES").Index("IDX_WORKFLOWPROCESSINSTANCES_S").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
