using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000010)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateObjects.sql")]
    public class Migration2000010CreateObjects : Migration
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
