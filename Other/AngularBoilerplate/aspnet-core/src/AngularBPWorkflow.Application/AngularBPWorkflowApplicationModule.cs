using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AngularBPWorkflow.Authorization;

namespace AngularBPWorkflow
{
    [DependsOn(
        typeof(AngularBPWorkflowCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class AngularBPWorkflowApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<AngularBPWorkflowAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(AngularBPWorkflowApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddProfiles(thisAssembly)
            );
        }
    }
}
