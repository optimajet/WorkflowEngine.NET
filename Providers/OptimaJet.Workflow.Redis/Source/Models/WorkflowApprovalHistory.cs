using System;
using System.Data;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Redis
{
    public class WorkflowApprovalHistory
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public string AllowedTo { get; set; }
        public DateTime? TransitionTime { get; set; }
        public long Sort { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string TriggerName { get; set; }
        public string Commentary { get; set; }
        
        public static WorkflowApprovalHistory ToDB(ApprovalHistoryItem historyItem)
        {
            return new WorkflowApprovalHistory()
            {
                Id = historyItem.Id,
                ProcessId = historyItem.ProcessId,
                IdentityId = historyItem.IdentityId,
                AllowedTo = HelperParser.Join(",", historyItem.AllowedTo),
                TransitionTime = historyItem.TransitionTime,
                Sort = historyItem.Sort,
                InitialState = historyItem.InitialState,
                DestinationState = historyItem.DestinationState,
                TriggerName = historyItem.TriggerName,
                Commentary = historyItem.Commentary,
            };
        }
        public static ApprovalHistoryItem FromDB(WorkflowRuntime runtime, WorkflowApprovalHistory historyItem)
        {
            return new ApprovalHistoryItem()
            {
                Id = historyItem.Id,
                ProcessId = historyItem.ProcessId,
                IdentityId = historyItem.IdentityId,
                AllowedTo = HelperParser.SplitWithTrim(historyItem.AllowedTo, ","),
                TransitionTime = runtime.ToRuntimeTime(historyItem.TransitionTime),
                Sort = historyItem.Sort,
                InitialState = historyItem.InitialState,
                DestinationState = historyItem.DestinationState,
                TriggerName = historyItem.TriggerName,
                Commentary = historyItem.Commentary,
            };
        }
    }

}
