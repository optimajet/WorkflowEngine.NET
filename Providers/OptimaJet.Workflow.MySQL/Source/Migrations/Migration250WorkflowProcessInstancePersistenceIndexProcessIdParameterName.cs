using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(250)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessInstancePersistence_ProcessId_ParameterName.sql")]
[ProductVersion("WorkflowEngine", "14.0.0")]
public class Migration250WorkflowProcessInstancePersistenceIndexProcessIdParameterName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessinstancepersistence").Index("ix_workflowprocessinstancepersistence_processId_parameterName").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
