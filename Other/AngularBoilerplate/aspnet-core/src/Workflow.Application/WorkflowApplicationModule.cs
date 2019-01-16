using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Workflow.Authorization;

namespace Workflow
{
    [DependsOn(
        typeof(WorkflowCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class WorkflowApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<WorkflowAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(WorkflowApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddProfiles(thisAssembly)
            );
        }
    }
}
