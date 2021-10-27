using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessTimer : DynamicEntity
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string Name { get; set; }
        public DateTime NextExecutionDateTime { get; set; }
        public bool Ignore { get; set; }
        public Guid RootProcessId { get; set; }
    }
}
