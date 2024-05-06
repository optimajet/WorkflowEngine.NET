using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000070)]
    public class Migration2000070StructDivisionParentsAndThisView : Migration
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
