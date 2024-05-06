using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000080)]
    public class Migration2000080EmployeeRole : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_EmployeeRole.sql"));
        }

        public override void Down()
        {
        }
    }
}
