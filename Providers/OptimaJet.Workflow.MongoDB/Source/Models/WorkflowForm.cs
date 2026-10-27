using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OptimaJet.Workflow.MongoDB.Models;

public class WorkflowForm : DynamicEntity
{
    [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Version { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string Definition { get; set; }
    public int Lock { get; set; }
    public string? TenantId { get; set; }

    public OptimaJet.Workflow.Core.Persistence.WorkflowForm ToModel()
    {
        return new OptimaJet.Workflow.Core.Persistence.WorkflowForm
        {
            Id = Id,
            CreationDate = CreationDate,
            Definition = Definition,
            Lock = Lock,
            Name = Name,
            TenantId = TenantId,
            UpdatedDate = UpdatedDate,
            Version = Version
        };
    }
}
