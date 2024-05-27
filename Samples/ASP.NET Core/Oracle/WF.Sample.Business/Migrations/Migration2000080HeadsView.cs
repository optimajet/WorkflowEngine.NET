using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000080)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateView_Heads.sql")]
    public class Migration2000080HeadsView : Migration
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
