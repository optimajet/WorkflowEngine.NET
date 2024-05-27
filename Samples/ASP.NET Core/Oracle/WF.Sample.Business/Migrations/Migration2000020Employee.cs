using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000020)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_Employee.sql")]
    public class Migration2000020Employee : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("EMPLOYEE").Exists())
            {
                this.EmbeddedScript();
            }
        }

        public override void Down()
        {
        }
    }
}
