using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000060)]
    public class Migration2000060StructDivisionParentsView : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateView_StructDivisionParents.sql"));
        }

        public override void Down()
        {
        }
    }
}
