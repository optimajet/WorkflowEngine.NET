using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using Workflow.Authorization.Roles;
using Workflow.Authorization.Users;
using Workflow.MultiTenancy;

namespace Workflow.EntityFrameworkCore
{
    public class WorkflowDbContext : AbpZeroDbContext<Tenant, Role, User, WorkflowDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
            : base(options)
        {
        }
    }
}
