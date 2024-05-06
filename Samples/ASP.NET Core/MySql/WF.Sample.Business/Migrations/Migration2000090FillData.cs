using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000090)]
    public class Migration2000090FillData : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("FillData.sql"));
        }

        public override void Down()
        {
        }
    }
}
