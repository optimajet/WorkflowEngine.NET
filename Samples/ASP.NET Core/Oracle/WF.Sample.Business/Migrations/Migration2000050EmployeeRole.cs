using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000050)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_EmployeeRole.sql")]
    public class Migration2000050EmployeeRole : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("EMPLOYEEROLE").Exists())
            {
                this.EmbeddedScript();
            }
        }

        public override void Down()
        {
        }
    }
}
