using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000070)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateView_StructDivisionParentsAndThis.sql")]
    public class Migration2000070StructDivisionParentsAndThisView : Migration
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
