using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000050)]
    public class Migration2000050Heads : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateView_Heads.sql"));
        }

        public override void Down()
        {
        }
    }
}
