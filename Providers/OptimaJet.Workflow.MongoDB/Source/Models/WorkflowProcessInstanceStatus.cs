using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessInstanceStatus : DynamicEntity
    {
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid Lock { get; set; }
        public byte Status { get; set; }

        public string RuntimeId { get; set; }

        public DateTime SetTime { get; set; }
    }
}
