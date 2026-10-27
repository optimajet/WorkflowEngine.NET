using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class ProcessInstanceTreeItem : IProcessInstanceTreeItem
    {
        private ProcessInstanceTreeItem()
        {}
        public Guid Id { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string SubprocessName { get; set; }
        public string StartingTransition { get; set; }

        public static List<IProcessInstanceTreeItem> CreateFromBsonDocuments(List<BsonDocument> instances, List<BsonDocument> schemes)
        {
            var result = instances.Join(
                schemes,
                i => i[nameof(WorkflowProcessInstance.SchemeId)].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy),
                s => s["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy),
                (i, s) => new ProcessInstanceTreeItem()
                {
                    Id = i["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy),
                    ParentProcessId = i[nameof(WorkflowProcessInstance.ParentProcessId)].IsBsonNull ? (Guid?)null : i[nameof(WorkflowProcessInstance.ParentProcessId)].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy),
                    RootProcessId = i[nameof(WorkflowProcessInstance.RootProcessId)].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy),
                    SubprocessName = i[nameof(WorkflowProcessInstance.SubprocessName)] == BsonNull.Value ? null : i[nameof(WorkflowProcessInstance.SubprocessName)].AsString,
                    StartingTransition = s[nameof(WorkflowProcessScheme.StartingTransition)] == BsonNull.Value ? null : s[nameof(WorkflowProcessScheme.StartingTransition)].AsString
                } as IProcessInstanceTreeItem).ToList();

            if (result.Count == instances.Count)
            {
                return result;
            }

            var mappedProcessIds = result.Select(p => p.Id).ToList();

            var unmappedDocumentIds = instances.Where(i => !mappedProcessIds.Contains(i[nameof(WorkflowProcessInstance.Id)].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy)))
                .Select(i => i[nameof(WorkflowProcessInstance.Id)].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy)).ToList();
            
            throw new Exception($"Can't create process instance tree. Unable to find schemes with the following id: {String.Join(",", unmappedDocumentIds)} ");
        }
    }
}
