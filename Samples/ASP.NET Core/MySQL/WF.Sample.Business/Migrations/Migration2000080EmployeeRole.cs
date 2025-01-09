using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000080)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_EmployeeRole.sql")]
    public class Migration2000080EmployeeRole : Migration
    {
        public override void Up()
        {
            this.EmbeddedScript();
        }

        public override void Down()
        {
        }
    }
}
