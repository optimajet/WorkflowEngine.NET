using System;

namespace OptimaJet.Workflow.Redis
{
    public class WorkflowInbox
    {
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }

    }
}
