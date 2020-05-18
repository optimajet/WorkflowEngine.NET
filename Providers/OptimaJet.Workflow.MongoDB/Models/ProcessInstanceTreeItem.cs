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
        public string StartingTransition { get; set; }

        public static List<IProcessInstanceTreeItem> CreateFromBsonDocuments(List<BsonDocument> instances, List<BsonDocument> schemes)
        {
            var result = instances.Join(
                schemes,
                i => i[nameof(WorkflowProcessInstance.SchemeId)].AsGuid,
                s => s["_id"].AsGuid,
                (i, s) => new ProcessInstanceTreeItem()
                {
                    Id = i["_id"].AsGuid,
                    ParentProcessId = i[nameof(WorkflowProcessInstance.ParentProcessId)].AsNullableGuid,
                    RootProcessId = i[nameof(WorkflowProcessInstance.RootProcessId)].AsGuid,
                    StartingTransition = s[nameof(WorkflowProcessScheme.StartingTransition)] == BsonNull.Value ? null : s[nameof(WorkflowProcessScheme.StartingTransition)].AsString
                } as IProcessInstanceTreeItem).ToList();

            if (result.Count == instances.Count)
            {
                return result;
            }

            var mappedProcessIds = result.Select(p => p.Id).ToList();

            var unmappedDocumentIds = instances.Where(i => !mappedProcessIds.Contains(i[nameof(WorkflowProcessInstance.Id)].AsGuid))
                .Select(i => i[nameof(WorkflowProcessInstance.Id)].AsGuid).ToList();
            
            throw new Exception($"Can't create process instance tree. Unable to find schemes with the following id: {String.Join(",", unmappedDocumentIds)} ");
        }
    }
}
