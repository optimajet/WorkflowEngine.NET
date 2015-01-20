using System;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.Services;
using Budget2.Server.Workflow.Interface.DataContracts;
using Budget2.Server.Workflow.Interface.Services;
using Common.WCSF;
using Microsoft.Practices.CompositeWeb;

namespace Budget2.Server.Workflow.Services
{
    public class BillDemandWorkflowService : WorkflowExternalDataExchangeService, IBillDemandWorkflowService
    {
        public event EventHandler<WorkflowEventArgsWithInitiator> PostingAccounting;
        public event EventHandler<WorkflowEventArgsWithInitiator> CheckStatus;
        public event EventHandler<PaidCommandEventArgs> SetPaidStatus;
        public event EventHandler<WorkflowEventArgsWithInitiator> SetDenialStatus;
        public event EventHandler<WorkflowEventArgsWithInitiator> Export;

        public void RaisePostingAccounting(WorkflowEventArgsWithInitiator args)
        {
            PostingAccounting(null, args);
        }

        public void RaiseCheckStatus(WorkflowEventArgsWithInitiator args)
        {
            CheckStatus(null, args);
        }

        public void RaiseSetPaidStatus(PaidCommandEventArgs args)
        {
            SetPaidStatus(null, args);
        }

        public void RaiseSetDenialStatus(DenialCommandEventArgs args)
        {
            SetDenialStatus(null, args);
        }

        public void RaiseExport(WorkflowEventArgsWithInitiator args)
        {
            Export(null, args);
        }

    }
}
