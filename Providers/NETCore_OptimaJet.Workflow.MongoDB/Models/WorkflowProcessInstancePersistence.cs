// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessInstancePersistence : DynamicEntity
    {
        public string ParameterName { get; set; }
        public string Value { get; set; }
    }
}
