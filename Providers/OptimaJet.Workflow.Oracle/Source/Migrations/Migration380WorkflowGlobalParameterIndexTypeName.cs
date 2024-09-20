using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(380)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.DropIndex_WorkflowGlobalParameter_TypeName.sql")]
public class Migration380WorkflowGlobalParameterIndexTypeName : Migration
{
    public override void Up()
    {
        if (Schema.Table("WORKFLOWGLOBALPARAMETER").Index("IDX_WORKFLOWGLOBALPARAMETER_TY").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
