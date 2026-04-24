using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(340)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowProcessInstance_IsDeterminingParametersChanged.sql")]
public class Migration340AlterColumnWorkflowProcessInstanceIsDeterminingParametersChanged : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
