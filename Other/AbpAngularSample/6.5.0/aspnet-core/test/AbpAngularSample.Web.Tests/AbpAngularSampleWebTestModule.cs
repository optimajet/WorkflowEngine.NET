using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AbpAngularSample.EntityFrameworkCore;
using AbpAngularSample.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace AbpAngularSample.Web.Tests
{
    [DependsOn(
        typeof(AbpAngularSampleWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class AbpAngularSampleWebTestModule : AbpModule
    {
        public AbpAngularSampleWebTestModule(AbpAngularSampleEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpAngularSampleWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(AbpAngularSampleWebMvcModule).Assembly);
        }
    }
}