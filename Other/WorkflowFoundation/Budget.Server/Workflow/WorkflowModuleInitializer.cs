using System;
using System.Collections.Generic;
using System.Text;
using Budget2.Server.Workflow.Interface.Services;
using Budget2.Server.Workflow.Services;
using Microsoft.Practices.CompositeWeb;
using Microsoft.Practices.CompositeWeb.Interfaces;
using Microsoft.Practices.CompositeWeb.Services;
using Microsoft.Practices.CompositeWeb.Configuration;
using Microsoft.Practices.CompositeWeb.EnterpriseLibrary.Services;

namespace Budget2.Server.Workflow
{
    public class WorkflowModuleInitializer : ModuleInitializer
    {
        public override void Load(CompositionContainer container)
        {
            base.Load(container);

            AddGlobalServices(container.Services);
        }

        protected virtual void AddGlobalServices(IServiceCollection globalServices)
        {
            globalServices.AddNew<BillDemandWorkflowService, IBillDemandWorkflowService>();
            globalServices.AddNew<WorkflowParcelService, IWorkflowParcelService>();
            globalServices.AddNew<WorkflowInitService, IWorkflowInitService>();
            
            
            
        }

        public override void Configure(IServiceCollection services, System.Configuration.Configuration moduleConfiguration)
        {
        }
    }
}
