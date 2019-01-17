using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.RavenDB
{
    public class WorkflowProcessInstanceStatus
    {
        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }
    }
}
