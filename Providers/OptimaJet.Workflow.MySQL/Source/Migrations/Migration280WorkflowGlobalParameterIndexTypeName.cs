using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(280)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowGlobalParameter_TypeName_2.sql")]
public class Migration280WorkflowGlobalParameterIndexTypeName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowglobalparameter").Index("ix_workflowglobalparameter_type_name").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
