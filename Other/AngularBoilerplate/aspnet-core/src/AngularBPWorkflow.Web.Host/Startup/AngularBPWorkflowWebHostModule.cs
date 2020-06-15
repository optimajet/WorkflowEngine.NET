using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AngularBPWorkflow.Configuration;

namespace AngularBPWorkflow.Web.Host.Startup
{
    [DependsOn(
       typeof(AngularBPWorkflowWebCoreModule))]
    public class AngularBPWorkflowWebHostModule: AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public AngularBPWorkflowWebHostModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AngularBPWorkflowWebHostModule).GetAssembly());
        }
    }
}
