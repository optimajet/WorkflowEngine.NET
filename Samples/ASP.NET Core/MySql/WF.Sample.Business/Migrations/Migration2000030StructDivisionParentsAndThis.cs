using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000030)]
    public class Migration2000030StructDivisionParentsAndThis : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateView_StructDivisionParentsAndThis.sql"));
        }

        public override void Down()
        {
        }
    }
}
