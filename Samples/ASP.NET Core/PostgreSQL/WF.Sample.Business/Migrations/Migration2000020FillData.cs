using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000020)]
    public class Migration2000020FillData : Migration
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
