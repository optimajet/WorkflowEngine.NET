using System;
using System.Collections.Generic;
using System.Text;
using Budget2.Server.Security.Interface.Services;
using Budget2.Server.Security.Services;
using Microsoft.Practices.CompositeWeb;
using Microsoft.Practices.CompositeWeb.Interfaces;
using Microsoft.Practices.CompositeWeb.Services;
using Microsoft.Practices.CompositeWeb.Configuration;
using Microsoft.Practices.CompositeWeb.EnterpriseLibrary.Services;
using IAuthorizationService = Budget2.Server.Security.Interface.Services.IAuthorizationService;

namespace Budget2.Server.Security
{
    public class SecurityModuleInitializer : ModuleInitializer
    {
        public override void Load(CompositionContainer container)
        {
            base.Load(container);

            AddGlobalServices(container.Services);
        }

        protected virtual void AddGlobalServices(IServiceCollection globalServices)
        {
           globalServices.AddNew<AuthorizationService, IAuthorizationService>();
        }

        public override void Configure(IServiceCollection services, System.Configuration.Configuration moduleConfiguration)
        {
        }
    }
}
