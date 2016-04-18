using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.RavenDB
{
    public class WorkflowProcessTimer
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string Name { get; set; }
        public DateTime NextExecutionDateTime { get; set; }
        public bool Ignore { get; set; } 
    }
}
