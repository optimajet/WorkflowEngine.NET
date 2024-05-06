using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000010)]
    public class Migration2000010StructDivision : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("STRUCTDIVISION").Exists())
            {
                Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_StructDivision.sql"));
            }
        }

        public override void Down()
        {
        }
    }
}
