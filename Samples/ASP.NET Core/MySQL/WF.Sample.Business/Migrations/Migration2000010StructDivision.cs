using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000010)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_StructDivision.sql")]
    public class Migration2000010StructDivision : Migration
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
