using Microsoft.Extensions.Configuration;
using Castle.MicroKernel.Registration;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AngularBPWorkflow.Configuration;
using AngularBPWorkflow.EntityFrameworkCore;
using AngularBPWorkflow.Migrator.DependencyInjection;

namespace AngularBPWorkflow.Migrator
{
    [DependsOn(typeof(AngularBPWorkflowEntityFrameworkModule))]
    public class AngularBPWorkflowMigratorModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public AngularBPWorkflowMigratorModule(AngularBPWorkflowEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

            _appConfiguration = AppConfigurations.Get(
                typeof(AngularBPWorkflowMigratorModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                AngularBPWorkflowConsts.ConnectionStringName
            );

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.ReplaceService(
                typeof(IEventBus), 
                () => IocManager.IocContainer.Register(
                    Component.For<IEventBus>().Instance(NullEventBus.Instance)
                )
            );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AngularBPWorkflowMigratorModule).GetAssembly());
            ServiceCollectionRegistrar.Register(IocManager);
        }
    }
}
