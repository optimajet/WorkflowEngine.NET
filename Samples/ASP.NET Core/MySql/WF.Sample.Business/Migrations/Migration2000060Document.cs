using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000060)]
    public class Migration2000060Document : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_Document.sql"));
        }

        public override void Down()
        {
        }
    }
}
