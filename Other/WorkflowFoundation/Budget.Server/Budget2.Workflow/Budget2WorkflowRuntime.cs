using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Workflow.Interface.Services;
using Budget2.Workflow.Tracking;
using Common;
using Common.WF;
using System.Collections.Specialized;
using System.Configuration;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;

namespace Budget2.Workflow
{
    public static class Budget2WorkflowRuntime
    {
        public static WorkflowRuntime Runtime
        {
            get;
            private set;
        }

        static Budget2WorkflowRuntime()
        {
            Runtime = new WorkflowRuntime();

            var persistenceParameters = new NameValueCollection();
            persistenceParameters["ConnectionString"] =
                ConfigurationManager.ConnectionStrings["default"].ConnectionString;
            persistenceParameters["UnloadOnIdle"] = "true";

            SqlWorkflowPersistenceService persistence = new NotTerminatingSqlWorkflowPersistenceService(persistenceParameters);

            Runtime.AddService(persistence);
            var trackingService = new Budget2TrackingService();
            Runtime.AddService(trackingService);
            Runtime.WorkflowTerminated += new System.EventHandler<WorkflowTerminatedEventArgs>(Runtime_WorkflowTerminated);
            Runtime.StartRuntime();
        }

        static void Runtime_WorkflowTerminated(object sender, WorkflowTerminatedEventArgs e)
        {
            Logger.Log.Error(string.Format("Ошибка маршрута Id={0} ({1})", e.WorkflowInstance.InstanceId, e.Exception));
        }

        static volatile object _sync = new object();

        static ExternalDataExchangeService _externalDataExchangeService;

        static ExternalDataExchangeService ExternalDataExchangeService
        {
            get
            {
                if (_externalDataExchangeService == null)
                {
                    lock (_sync)
                    {
                        if (_externalDataExchangeService == null)
                        {
                            _externalDataExchangeService = new ExternalDataExchangeService();
                            Runtime.AddService(_externalDataExchangeService);
                        }
                    }

                }
                return _externalDataExchangeService;
            }
        }

        public static void AddExternalDataExchangeService<T>(T service)
        {
            ExternalDataExchangeService.AddService(service);
        }

        public static IBillDemandBuinessService BillDemandBuinessService
        {
            get
            {
                return Runtime.GetService<IBillDemandBuinessService>();
            }
        }

        public static IDemandAdjustmentBusinessService DemandAdjustmentBusinessService
        {
            get
            {
                return Runtime.GetService<IDemandAdjustmentBusinessService>();
            }
        }

        public static IBillDemandExportService BillDemandExportService
        {
            get
            {
                return Runtime.GetService<IBillDemandExportService>();
            }
        }

        public static IDemandBusinessService DemandBusinessService
        {
            get
            {
                return Runtime.GetService<IDemandBusinessService>();
            }
        }

        public static IBillDemandNotificationService BillDemandNotificationService
        {
            get
            {
                return Runtime.GetService<IBillDemandNotificationService>();
            }
        }

        public static IWorkflowParcelService WorkflowParcelService
        {
            get
            {
                return Runtime.GetService<IWorkflowParcelService>();
            }
        }

        public static IDemandNotificationService DemandNotificationService
        {
            get
            {
                return Runtime.GetService<IDemandNotificationService>();
            }
        }

    }
}
