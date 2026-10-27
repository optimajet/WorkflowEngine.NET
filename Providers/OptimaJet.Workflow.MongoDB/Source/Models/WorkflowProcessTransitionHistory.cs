using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessTransitionHistory : DynamicEntity
    {
        public string ActorIdentityId { get; set; }
        public string ExecutorIdentityId { get; set; }
        public string ActorName{ get; set; }
        public string ExecutorName { get; set; }
        public string FromActivityName { get; set; }
        public string FromStateName { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid Id { get; set; }
        public bool IsFinalised { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid ProcessId { get; set; }
        public string TenantId { get; set; }
        public string ToActivityName { get; set; }
        public string ToStateName { get; set; }
        public string TransitionClassifier { get; set; }
        public DateTime TransitionTime { get; set; }
        public string TriggerName { get; set; }
        public DateTime? StartTransitionTime { get; set; }
        public long? TransitionDuration { get; set; }
    }
}
