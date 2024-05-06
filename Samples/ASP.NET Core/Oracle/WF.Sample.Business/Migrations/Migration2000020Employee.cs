using FluentMigrator;

namespace WF.Sample.Business.Migrations
{
    [Migration(2000020)]
    public class Migration2000020Employee : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("EMPLOYEE").Exists())
            {
                Execute.EmbeddedScript(MigrationUtil.GetEmbeddedPath("CreateTable_Employee.sql"));
            }
        }

        public override void Down()
        {
        }
    }
}
