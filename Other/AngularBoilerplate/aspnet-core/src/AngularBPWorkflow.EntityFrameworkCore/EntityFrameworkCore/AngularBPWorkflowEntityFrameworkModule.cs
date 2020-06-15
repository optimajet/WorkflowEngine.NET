using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using AngularBPWorkflow.EntityFrameworkCore.Seed;

namespace AngularBPWorkflow.EntityFrameworkCore
{
    [DependsOn(
        typeof(AngularBPWorkflowCoreModule), 
        typeof(AbpZeroCoreEntityFrameworkCoreModule))]
    public class AngularBPWorkflowEntityFrameworkModule : AbpModule
    {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<AngularBPWorkflowDbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        AngularBPWorkflowDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    }
                    else
                    {
                        AngularBPWorkflowDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                    }
                });
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AngularBPWorkflowEntityFrameworkModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            if (!SkipDbSeed)
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }
    }
}
