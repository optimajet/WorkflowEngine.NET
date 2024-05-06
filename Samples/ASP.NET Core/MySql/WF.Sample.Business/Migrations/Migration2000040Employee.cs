using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000040)]
    public class Migration2000040Employee : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_Employee.sql"));
        }

        public override void Down()
        {
        }
    }
}
