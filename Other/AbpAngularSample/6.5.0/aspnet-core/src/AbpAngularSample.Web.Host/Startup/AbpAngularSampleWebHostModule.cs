using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AbpAngularSample.Configuration;

namespace AbpAngularSample.Web.Host.Startup
{
    [DependsOn(
       typeof(AbpAngularSampleWebCoreModule))]
    public class AbpAngularSampleWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public AbpAngularSampleWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpAngularSampleWebHostModule).GetAssembly());
        }
    }
}
