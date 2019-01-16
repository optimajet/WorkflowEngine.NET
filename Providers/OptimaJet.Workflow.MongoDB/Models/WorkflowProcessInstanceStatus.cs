using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessInstanceStatus : DynamicEntity
    {
        public Guid Lock { get; set; }
        public byte Status { get; set; }
    }
}
