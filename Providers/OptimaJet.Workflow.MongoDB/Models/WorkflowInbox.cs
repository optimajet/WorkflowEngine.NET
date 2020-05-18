using System;

namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowInbox : DynamicEntity
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }

    }
}
