using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using AngularBPWorkflow.Configuration;
using AngularBPWorkflow.Web;

namespace AngularBPWorkflow.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class AngularBPWorkflowDbContextFactory : IDesignTimeDbContextFactory<AngularBPWorkflowDbContext>
    {
        public AngularBPWorkflowDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AngularBPWorkflowDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            AngularBPWorkflowDbContextConfigurer.Configure(builder, configuration.GetConnectionString(AngularBPWorkflowConsts.ConnectionStringName));

            return new AngularBPWorkflowDbContext(builder.Options);
        }
    }
}
