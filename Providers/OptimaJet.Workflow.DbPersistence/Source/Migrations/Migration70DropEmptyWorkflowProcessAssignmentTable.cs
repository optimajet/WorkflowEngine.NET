using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(70)]
[WorkflowEngineMigration("OptimaJet.Workflow.MSSQL.Scripts.DropEmptyWorkflowProcessAssignmentTable.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration70DropEmptyWorkflowProcessAssignmentTable : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
