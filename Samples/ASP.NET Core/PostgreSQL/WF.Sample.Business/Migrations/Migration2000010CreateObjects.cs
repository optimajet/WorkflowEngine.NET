using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000010)]
    public class Migration2000010CreateObjects : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateObjects.sql"));
        }

        public override void Down()
        {
        }
    }
}
