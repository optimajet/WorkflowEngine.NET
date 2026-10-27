using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MSSQL.Migrations;

[Migration(60)]
[WorkflowEngineMigration("OptimaJet.Workflow.MSSQL.Scripts.RemoveAssignmentCheckDeadlinesSystemProcess.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration60RemoveAssignmentCheckDeadlinesSystemProcess : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
