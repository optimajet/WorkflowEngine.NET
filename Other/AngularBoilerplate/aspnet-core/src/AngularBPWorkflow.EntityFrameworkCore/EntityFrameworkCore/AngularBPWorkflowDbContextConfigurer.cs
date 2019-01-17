using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace AngularBPWorkflow.EntityFrameworkCore
{
    public static class AngularBPWorkflowDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AngularBPWorkflowDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<AngularBPWorkflowDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
