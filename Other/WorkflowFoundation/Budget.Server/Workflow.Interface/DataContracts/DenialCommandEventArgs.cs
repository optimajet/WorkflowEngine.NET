using System;

namespace Budget2.Server.Workflow.Interface.DataContracts
{
    [Serializable]
    public class DenialCommandEventArgs : WorkflowEventArgsWithInitiator
    {
        public string Comment { get; set; }

        public DenialCommandEventArgs(Guid instanceId, Guid initiatorId)
            : base(instanceId, initiatorId)
        {
        }

        public DenialCommandEventArgs(Guid instanceId, Guid initiatorId, Guid? impersonatedIdentityId)
            : base(instanceId, initiatorId, impersonatedIdentityId)
        {
        }
    }
}
