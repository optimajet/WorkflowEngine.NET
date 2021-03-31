using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using AutoMapper;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Business.Workflow;
using WF.Sample.Models;
using WF.Sample.ServiceLocation;
using WorkflowRuntime = OptimaJet.Workflow.Core.Runtime.WorkflowRuntime;

namespace WF.Sample
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public WorkflowRuntime Runtime;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Document", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterMappings();
            var container = ConfigureContainer();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            Runtime = WorkflowInit.Create(new DataServiceProvider(container));
        }

        private void RegisterMappings()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DocumentModel, Document>()
                    .ForMember(d => d.Author, o => o.MapFrom(s => new Employee { Id = s.AuthorId, Name = s.AuthorName }))
                    .ForMember(d => d.Manager, o => o.MapFrom(s => s.ManagerId.HasValue ?
                        new Employee { Id = s.ManagerId.Value, Name = s.ManagerName } :
                        null))
                    ;
            });
        }

        private IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterModule(new ConfigurationSettingsReader());
          
            var container = builder.Build();
 
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;

        }
    }
}
