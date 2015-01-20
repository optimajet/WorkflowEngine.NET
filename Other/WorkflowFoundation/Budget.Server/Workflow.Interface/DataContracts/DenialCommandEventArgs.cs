using System;
using System.Collections.Generic;
using System.Workflow.Activities;

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
     [Serializable]
    public class SetWorkflowInternalParametersEventArgs : ExternalDataEventArgs
    {
        public Dictionary<string, object> Parameters { get; private set; } 

        public SetWorkflowInternalParametersEventArgs(Guid instanceId,Dictionary<string,object> parameters)
            : base(instanceId)
        {
            Parameters = parameters;
        }
    }
}
