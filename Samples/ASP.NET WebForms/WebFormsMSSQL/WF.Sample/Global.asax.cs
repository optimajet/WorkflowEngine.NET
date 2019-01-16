using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Web;
using AutoMapper;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Business.Workflow;
using WF.Sample.Models;
using WF.Sample.ServiceLocation;

namespace WF.Sample
{
    public class Global : HttpApplication, IContainerProviderAccessor
    {
        public WorkflowRuntime Runtime { get; private set; }

        static IContainerProvider _containerProvider;

        public IContainerProvider ContainerProvider => _containerProvider;

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RegisterMappings();

            var container = ConfigureContainer();
            
            Runtime = WorkflowInit.Create(new DataServiceProvider(container));
            _containerProvider = new ContainerProvider(container);
        }

        private IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationSettingsReader());

            var container = builder.Build();

            return container;
        }

        private void RegisterMappings()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DocumentModel, WF.Sample.Business.Model.Document>()
                .ForMember(d => d.Author, o => o.MapFrom(s => new Employee { Id = s.AuthorId, Name = s.AuthorName }))
                   .ForMember(d => d.Manager, o => o.MapFrom(s => s.ManagerId.HasValue ?
                        new Employee { Id = s.ManagerId.Value, Name = s.ManagerName } :
                        null))
                ;
            });
        }
    }
}