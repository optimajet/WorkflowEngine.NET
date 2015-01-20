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
    }
}
