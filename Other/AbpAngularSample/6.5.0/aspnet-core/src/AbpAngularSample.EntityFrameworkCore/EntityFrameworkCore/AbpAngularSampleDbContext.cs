using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using AbpAngularSample.Authorization.Roles;
using AbpAngularSample.Authorization.Users;
using AbpAngularSample.MultiTenancy;

namespace AbpAngularSample.EntityFrameworkCore
{
    public class AbpAngularSampleDbContext : AbpZeroDbContext<Tenant, Role, User, AbpAngularSampleDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public AbpAngularSampleDbContext(DbContextOptions<AbpAngularSampleDbContext> options)
            : base(options)
        {
        }
    }
}
