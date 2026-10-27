using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessScheme : DynamicEntity
    {
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid Id { get; set; }
        public bool IsObsolete { get; set; }
        public string SchemeCode { get; set; }
        public string Scheme { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid? RootSchemeId { get; set; }
        public string RootSchemeCode { get; set; }
        public List<string> AllowedActivities { get; set; }
        public string StartingTransition { get; set; }
        public string TenantId { get; set; }
    }
}
