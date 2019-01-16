using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Workflow.Configuration;
using Workflow.Web;

namespace Workflow.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class WorkflowDbContextFactory : IDesignTimeDbContextFactory<WorkflowDbContext>
    {
        public WorkflowDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<WorkflowDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            WorkflowDbContextConfigurer.Configure(builder, configuration.GetConnectionString(WorkflowConsts.ConnectionStringName));

            return new WorkflowDbContext(builder.Options);
        }
    }
}
