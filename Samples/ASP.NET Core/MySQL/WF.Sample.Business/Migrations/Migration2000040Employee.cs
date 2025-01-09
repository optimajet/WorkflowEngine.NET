using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000040)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_Employee.sql")]
    public class Migration2000040Employee : Migration
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
