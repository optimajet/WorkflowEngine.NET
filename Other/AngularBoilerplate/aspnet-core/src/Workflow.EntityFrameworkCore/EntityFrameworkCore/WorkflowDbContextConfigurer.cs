using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Workflow.EntityFrameworkCore
{
    public static class WorkflowDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<WorkflowDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<WorkflowDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
