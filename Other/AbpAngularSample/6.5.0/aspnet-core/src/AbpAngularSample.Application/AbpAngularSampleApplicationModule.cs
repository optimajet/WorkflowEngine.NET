using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AbpAngularSample.Authorization;

namespace AbpAngularSample
{
    [DependsOn(
        typeof(AbpAngularSampleCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class AbpAngularSampleApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<AbpAngularSampleAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(AbpAngularSampleApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
