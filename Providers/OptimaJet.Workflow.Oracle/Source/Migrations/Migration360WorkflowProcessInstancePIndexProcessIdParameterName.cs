using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(360)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessInstanceP_ProcessId_ParameterName.sql")]
public class Migration360WorkflowProcessInstancePIndexProcessIdParameterName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCEP").Index("IDX_WORKFLOWPROCESSINSTANCEP_PP").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
