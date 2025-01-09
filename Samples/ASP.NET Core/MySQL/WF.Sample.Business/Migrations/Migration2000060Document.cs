using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000060)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_Document.sql")]
    public class Migration2000060Document : Migration
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
