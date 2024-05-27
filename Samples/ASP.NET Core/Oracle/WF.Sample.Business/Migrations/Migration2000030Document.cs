using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000030)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_Document.sql")]
    public class Migration2000030Document : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("DOCUMENT").Exists())
            {
                this.EmbeddedScript();
            }
        }

        public override void Down()
        {
        }
    }
}
