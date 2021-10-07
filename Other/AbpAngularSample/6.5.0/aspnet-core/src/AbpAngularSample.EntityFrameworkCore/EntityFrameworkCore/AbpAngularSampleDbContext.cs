using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using AbpAngularSample.Authorization.Roles;
using AbpAngularSample.Authorization.Users;
using AbpAngularSample.Documents;
using AbpAngularSample.MultiTenancy;

namespace AbpAngularSample.EntityFrameworkCore
{
    public class AbpAngularSampleDbContext : AbpZeroDbContext<Tenant, Role, User, AbpAngularSampleDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        //WorkflowEngineSampleCode
        public DbSet<Document> Documents { get; set; }
        
        public AbpAngularSampleDbContext(DbContextOptions<AbpAngularSampleDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Document>(b =>
            {
                b.ToTable("AppDocuments");
            });
        }
    }
}
