using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(270)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.DropIndex_WorkflowGlobalParameter_TypeName.sql")]
public class Migration270WorkflowGlobalParameterDropIndexTypeName : Migration
{
    public override void Up()
    {
        if (Schema.Table("workflowglobalparameter").Index("ix_workflowglobalparameter_type_name").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
