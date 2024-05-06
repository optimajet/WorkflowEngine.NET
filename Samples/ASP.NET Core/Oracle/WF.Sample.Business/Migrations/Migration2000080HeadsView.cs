using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000080)]
    public class Migration2000080HeadsView : Migration
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
