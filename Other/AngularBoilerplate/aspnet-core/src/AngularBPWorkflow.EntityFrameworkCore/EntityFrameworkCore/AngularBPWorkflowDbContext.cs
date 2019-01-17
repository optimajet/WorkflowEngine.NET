using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using AngularBPWorkflow.Authorization.Roles;
using AngularBPWorkflow.Authorization.Users;
using AngularBPWorkflow.MultiTenancy;
using AngularBPWorkflow.Documents;

namespace AngularBPWorkflow.EntityFrameworkCore
{
    public class AngularBPWorkflowDbContext : AbpZeroDbContext<Tenant, Role, User, AngularBPWorkflowDbContext>
    {
        /* Define a DbSet for each entity of the application */

        //WorkflowEngineSampleCode
        public DbSet<Document> Documents { get; set; }

        public AngularBPWorkflowDbContext(DbContextOptions<AngularBPWorkflowDbContext> options)
            : base(options)
        {
        }
    }
}
