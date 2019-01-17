using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.RavenDB
{
    public class WorkflowGlobalParameter
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}