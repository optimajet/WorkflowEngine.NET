using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OptimaJet.Workflow.MongoDB.Models
{
    public class WorkflowSync : DynamicEntity
    {
        public string Name { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid Lock { get; set; }
    }
}
