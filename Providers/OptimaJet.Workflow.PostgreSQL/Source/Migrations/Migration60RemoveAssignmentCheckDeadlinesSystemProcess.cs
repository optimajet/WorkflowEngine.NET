using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.PostgreSQL.Migrations;

[Migration(60)]
[WorkflowEngineMigration("OptimaJet.Workflow.PostgreSQL.Scripts.RemoveAssignmentCheckDeadlinesSystemProcess.sql")]
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
