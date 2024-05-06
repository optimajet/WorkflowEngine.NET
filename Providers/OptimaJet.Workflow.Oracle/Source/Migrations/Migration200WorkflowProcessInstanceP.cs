using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(200)]
public class Migration200WorkflowProcessInstanceP : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSINSTANCEP").Exists())
        {
            this.EmbeddedScript("CreateTable_WorkflowProcessInstanceP.sql");
        }
    }

    public override void Down()
    {
    }
}
