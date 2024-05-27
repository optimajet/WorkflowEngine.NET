using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000020)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.FillData.sql")]
    public class Migration2000020FillData : Migration
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
