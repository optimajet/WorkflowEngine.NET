using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(270)]
public class Migration270WorkflowGlobalParameterIndexTypeName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWGLOBALPARAMETER").Index("IDX_WORKFLOWGLOBALPARAMETER_TY").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowGlobalParameter_TypeName.sql");
        }
    }

    public override void Down()
    {
    }
}
