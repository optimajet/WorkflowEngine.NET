using System;
using System.Workflow.Activities;

namespace Budget2.Server.Workflow.Interface.DataContracts
{
    [Serializable]
    [Obsolete]
    public class UPKZControllerSightingCommandEventArgs : WorkflowEventArgsWithInitiator
    {
        public UPKZControllerSightingCommandEventArgs(Guid instanceId, Guid limitManagerId, Guid upkzCuratorId, Guid initiatorId)
            : base(instanceId, initiatorId)
        {
            WaitForIdle = true;
            LimitManagerId = limitManagerId;
            UPKZCuratorId = upkzCuratorId;
        }

        public UPKZControllerSightingCommandEventArgs(Guid instanceId, Guid limitManagerId, Guid upkzCuratorId, Guid initiatorId, bool waitForIdle)
            : base(instanceId, initiatorId)
        {
            WaitForIdle = waitForIdle;
            LimitManagerId = limitManagerId;
            UPKZCuratorId = upkzCuratorId;
        }


        public Guid LimitManagerId { get; private set; }

        public Guid UPKZCuratorId { get; private set; }
    }
}
