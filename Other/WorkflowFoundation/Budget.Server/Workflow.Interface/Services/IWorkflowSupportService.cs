using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using Budget2.DAL.DataContracts;
using Budget2.Server.Workflow.Interface.DataContracts;

namespace Budget2.Server.Workflow.Interface.Services
{
    [ExternalDataExchange]
    public interface IWorkflowSupportService
    {
        void UpgradeWorkflow(WorkflowRuntime runtime, Guid instanceId);
        bool TryUpgradeWorkflow(WorkflowRuntime runtime, Guid instanceId);
        void RewriteWorkflow(Guid instanceId, WorkflowState state);
        void CreateWorkflowIfNotExists(WorkflowRuntime runtime, Guid instanceId);
        bool CreateWorkflowIfNotExists(WorkflowRuntime runtime, Guid instanceId, WorkflowType workflowType);

        bool CreateWorkflowIfNotExists(WorkflowRuntime runtime, Guid instanceId, WorkflowType workflowType,
                                       Dictionary<string, object> parameters);

        event EventHandler<SetWorkflowInternalParametersEventArgs> SetInternalParameters;
    }
}
