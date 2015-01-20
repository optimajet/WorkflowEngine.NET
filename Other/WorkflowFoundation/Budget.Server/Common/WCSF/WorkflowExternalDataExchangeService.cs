using System;
using System.Workflow.Activities;

namespace Common.WCSF
{
    public class WorkflowExternalDataExchangeService
    {
        protected ExternalDataEventArgs GetDefaultArgs(Guid instanceId)
        {
            var args = new ExternalDataEventArgs(instanceId) { WaitForIdle = true };
            return args;
        }
    }
}
