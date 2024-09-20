using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(390)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowGlobalParameter_TypeName_2.sql")]
public class Migration390WorkflowGlobalParameterIndexTypeName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWGLOBALPARAMETER").Index("IDX_WORKFLOWGLOBALPARAMETER_TY").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
