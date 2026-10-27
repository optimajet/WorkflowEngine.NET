using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowGlobalParameter : DynamicEntity
    {
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string TenantId { get; set; }
    }
}
