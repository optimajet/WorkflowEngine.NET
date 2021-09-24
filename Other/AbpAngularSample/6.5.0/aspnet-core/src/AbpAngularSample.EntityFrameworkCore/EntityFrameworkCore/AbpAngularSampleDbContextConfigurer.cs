using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace AbpAngularSample.EntityFrameworkCore
{
    public static class AbpAngularSampleDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AbpAngularSampleDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<AbpAngularSampleDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
