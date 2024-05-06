using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000070)]
    public class Migration2000070Roles : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_Roles.sql"));
        }

        public override void Down()
        {
        }
    }
}
