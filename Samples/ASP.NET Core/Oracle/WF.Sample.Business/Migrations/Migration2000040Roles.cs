using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000040)]
    [WorkflowEngineMigration("WF.Sample.Business.Scripts.CreateTable_Roles.sql")]
    public class Migration2000040Roles : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ROLES").Exists())
            {
                this.EmbeddedScript();
            }
        }

        public override void Down()
        {
        }
    }
}
