using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(210)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessInstanceP_ProcessId.sql")]
[ProductVersion("WorkflowEngine", "13.0.0")]
public class Migration210WorkflowProcessInstancePIndexProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCEP").Index("IDX_WORKFLOWPROCESSINSTANCEP_P").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
