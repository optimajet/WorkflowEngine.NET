using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(180)]
public class Migration180WorkflowGlobalParameterIndexTypeName : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowglobalparameter").Index("ix_workflowglobalparameter_type_name").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowGlobalParameter_TypeName.sql");
        }
    }

    public override void Down()
    {
    }
}
