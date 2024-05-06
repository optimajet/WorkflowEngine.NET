using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000030)]
    public class Migration2000030Document : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("DOCUMENT").Exists())
            {
                Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_Document.sql"));
            }
        }

        public override void Down()
        {
        }
    }
}
