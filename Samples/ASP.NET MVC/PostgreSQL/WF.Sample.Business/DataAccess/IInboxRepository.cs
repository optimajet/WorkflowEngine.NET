using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.Business.DataAccess
{
    public interface IInboxRepository
    {
        void DropWorkflowInbox(Guid processId);
        void FillInbox(Guid processId, WorkflowRuntime workflowRuntime);
        void RecalcInbox(WorkflowRuntime workflowRuntime);
    }
}
