using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Workflow.Configuration;

namespace Workflow.Web.Host.Startup
{
    [DependsOn(
       typeof(WorkflowWebCoreModule))]
    public class WorkflowWebHostModule: AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public WorkflowWebHostModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(WorkflowWebHostModule).GetAssembly());
        }
    }
}
