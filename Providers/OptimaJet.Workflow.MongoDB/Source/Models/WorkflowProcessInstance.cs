using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessInstance : DynamicEntity
    {
      
        public string ActivityName { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid Id { get; set; }
        public string PreviousActivity { get; set; }
        public string PreviousActivityForDirect { get; set; }
        public string PreviousActivityForReverse { get; set; }
        public string PreviousState { get; set; }
        public string PreviousStateForDirect { get; set; }
        public string PreviousStateForReverse { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid? SchemeId { get; set; }
        public string StateName { get; set; }
        public List<WorkflowProcessInstancePersistence> Persistence { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid? ParentProcessId { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid RootProcessId { get; set; }
        public string TenantId { get; set; }
        
        public string SubprocessName { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime? LastTransitionDate { get; set; }

        public WorkflowProcessInstanceStatus Status { get; set; }
        
        public string CalendarName { get; set; }
    }
}
