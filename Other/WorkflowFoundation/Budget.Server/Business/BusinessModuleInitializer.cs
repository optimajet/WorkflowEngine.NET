using System;
using System.Collections.Generic;
using System.Text;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Business.Services;
using Budget2.Server.Security.Interface.Services;
using Budget2.Server.Security.Services;
using Microsoft.Practices.CompositeWeb;
using Microsoft.Practices.CompositeWeb.Interfaces;
using Microsoft.Practices.CompositeWeb.Services;
using Microsoft.Practices.CompositeWeb.Configuration;
using Microsoft.Practices.CompositeWeb.EnterpriseLibrary.Services;

namespace Budget2.Server.Business
{
    public class BusinessModuleInitializer : ModuleInitializer
    {
        public override void Load(CompositionContainer container)
        {
            base.Load(container);

            AddGlobalServices(container.Services);
        }

        protected virtual void AddGlobalServices(IServiceCollection globalServices)
        {
            globalServices.AddNew<SettingsService, ISettingsService>();
            globalServices.AddNew<WorkflowTicketService, IWorkflowTicketService>();
            globalServices.AddNew<WorkflowStateService, IWorkflowStateService>();
            globalServices.AddNew<EmployeeService, IEmployeeService>();
            globalServices.AddNew<SecurityEntityService, ISecurityEntityService>();
            globalServices.AddNew<BillDemandBusinessService, IBillDemandBuinessService>();
            globalServices.AddNew<AuthenticationService, IAuthenticationService>();
            globalServices.AddNew<UpdateRatesBuinessService, IUpdateRatesService>();
            globalServices.AddNew<ContractUploadBuinessService, IContractUploadBuinessService>();
            globalServices.AddNew<CovenanteeUploadBuinessService, ICovenanteeUploadBuinessService>();
            globalServices.AddNew<DemandAdjustmentBusinessService, IDemandAdjustmentBusinessService>();
            globalServices.AddNew<BillDemandExportService,IBillDemandExportService>();
            globalServices.AddNew<DemandBusinessService, IDemandBusinessService>();
            globalServices.AddNew<EmailService, IEmailService>();
            globalServices.AddNew<DemandNotificationService, IDemandNotificationService>();
            globalServices.AddNew<BillDemandNotificationService, IBillDemandNotificationService>();
         
        }

        public override void Configure(IServiceCollection services, System.Configuration.Configuration moduleConfiguration)
        {
        }
    }
}
