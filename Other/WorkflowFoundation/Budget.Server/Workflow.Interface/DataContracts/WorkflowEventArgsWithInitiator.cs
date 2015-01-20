using System;
using System.Workflow.Activities;

namespace Budget2.Server.Workflow.Interface.DataContracts
{
    [Serializable]
    public class WorkflowEventArgsWithInitiator : ExternalDataEventArgs
    {
        public WorkflowEventArgsWithInitiator(Guid instanceId, Guid initiatorId)
            : this(instanceId,initiatorId,null)
        {
        }

        public WorkflowEventArgsWithInitiator(Guid instanceId, Guid initiatorId, Guid? impersonatedIdentityId)
            : base(instanceId)
        {
            InitiatorId = initiatorId;
            ImpersonatedIdentityId = impersonatedIdentityId;
            WaitForIdle = false;
        }

        public Guid InitiatorId { get; private set; }

        public Guid? ImpersonatedIdentityId { get; private set; }
    }
}
