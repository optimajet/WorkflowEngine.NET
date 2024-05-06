using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(50)]
public class Migration50WorkflowProcessInstanceIndexRootProcessId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCE").Index("IDX_WORKFLOWPROCESSINSTANCE_ROOT").Exists())
        {
            this.EmbeddedScript("CreateIndex_WorkflowProcessInstance_RootProcessId.sql");
        }
    }

    public override void Down()
    {
    }
}
