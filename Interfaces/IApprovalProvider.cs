using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Persistence
{
    public interface IApprovalProvider
    {
        Task DropWorkflowInboxAsync(Guid processId);
        Task InsertInboxAsync(Guid processId, List<string> newActors);
        Task WriteApprovalHistoryAsync(Guid id, string currentState, string nextState, string triggerName, string allowedToEmployeeNames, long order);
        Task UpdateApprovalHistoryAsync(Guid id, string currentState, string nextState, string triggerName, string identityId, long order, string comment);
        Task DropEmptyApprovalHistoryAsync(Guid processId);
    }
}
