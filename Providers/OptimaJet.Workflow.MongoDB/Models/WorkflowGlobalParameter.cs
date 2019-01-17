using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowGlobalParameter : DynamicEntity
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}