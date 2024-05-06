using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000050)]
    public class Migration2000050EmployeeRole : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("EMPLOYEEROLE").Exists())
            {
                Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_EmployeeRole.sql"));
            }
        }

        public override void Down()
        {
        }
    }
}
