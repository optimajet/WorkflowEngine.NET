using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(710)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.RemoveAssignmentCheckDeadlinesSystemProcess.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration710RemoveAssignmentCheckDeadlinesSystemProcess : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}
