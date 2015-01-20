using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using Budget2.Workflow;

namespace Common.WF
{
    public class NotTerminatingSqlWorkflowPersistenceService : SqlWorkflowPersistenceService
    {
        public class WorkflowSavedParametersArgs : EventArgs
        {
            public Dictionary<string, object> Parameters = new Dictionary<string, object>();
            public Guid InstanceId { get; set;}
        }

        public NotTerminatingSqlWorkflowPersistenceService(string connectionString) : base(connectionString) { }
        public NotTerminatingSqlWorkflowPersistenceService(NameValueCollection parameters) : base(parameters) { }
        public NotTerminatingSqlWorkflowPersistenceService(string connectionString, bool unloadOnIdle, TimeSpan instanceOwnerShipDuration, TimeSpan loadingInterval)
            : base(connectionString, unloadOnIdle, instanceOwnerShipDuration, loadingInterval) { }

        protected override void SaveWorkflowInstanceState(Activity rootActivity, bool unlock)
        {
            WorkflowStatus workflowStatus = WorkflowPersistenceService.GetWorkflowStatus(rootActivity);
            if (workflowStatus == WorkflowStatus.Terminated)
            {
                string workflowError = WorkflowPersistenceService.GetSuspendOrTerminateInfo(rootActivity);
                //Logger.Log.Error(workflowError);
                return;
            }
            base.SaveWorkflowInstanceState(rootActivity, unlock);
        }

        protected override Activity LoadWorkflowInstanceState(Guid id)
        {
            var act = base.LoadWorkflowInstanceState(id);
            if (!(act is StateMachineWithSimpleContainer))
                return act;
            var args = new WorkflowSavedParametersArgs() {InstanceId = id};
            foreach (var kvp in (act as StateMachineWithSimpleContainer).WorkflowPersistanceParameters)
            {
                args.Parameters.Add(kvp.Key,kvp.Value);
            }
            if (OnArgsAllowed != null)
                OnArgsAllowed(null, args);
            return act;
        }


        public EventHandler<WorkflowSavedParametersArgs> OnArgsAllowed;
        
    }
}
