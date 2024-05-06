using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000040)]
    public class Migration2000040Roles : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ROLES").Exists())
            {
                Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_Roles.sql"));
            }
        }

        public override void Down()
        {
        }
    }
}
