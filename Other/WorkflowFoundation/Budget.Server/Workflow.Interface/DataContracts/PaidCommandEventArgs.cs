using System;

namespace Budget2.Server.Workflow.Interface.DataContracts
{
    [Serializable]
    public  class PaidCommandEventArgs : WorkflowEventArgsWithInitiator
    {
        public DateTime? PaymentDate { get; set; }

        public string DocumentNumber { get; set; }

        public PaidCommandEventArgs(Guid instanceId, Guid initiatorId)
            : base(instanceId, initiatorId)
        {
        }

        public PaidCommandEventArgs(Guid instanceId, Guid initiatorId, Guid? impersonatedIdentityId)
            : base(instanceId, initiatorId, impersonatedIdentityId)
        {
        }
    }
}
