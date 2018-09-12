using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WF.Sample.Business.Helpers;
using WF.Sample.Business.Models;
using MongoDB.Driver;

namespace WF.Sample.Business.Workflow
{
    public  class WorkflowActions : IWorkflowActionProvider
    {
        public static void WriteTransitionHistory(ProcessInstance processInstance, string parameter)
        {
            if (processInstance.IdentityIds == null)
                return;

            var currentstate = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.CurrentState);

            var nextState = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);

            var command = WorkflowInit.Runtime.GetLocalizedCommandName(processInstance.ProcessId, processInstance.CurrentCommand);

            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            var doc = dbcoll.Find(x => x.Id == processInstance.ProcessId).FirstOrDefault();
            if (doc != null)
            {
                doc.TransitionHistories.Add(new DocumentTransitionHistory()
                {
                    Id = Guid.NewGuid(),
                    AllowedToEmployeeNames = GetEmployeesString(processInstance.IdentityIds),
                    DestinationState = nextState,
                    InitialState = currentstate,
                    Command = command
                });

                dbcoll.ReplaceOne(x => x.Id == doc.Id, doc, new UpdateOptions { IsUpsert = true });
            }
        }

        private static string GetEmployeesString(IEnumerable<string> identities)
        {
            var employees = EmployeeHelper.EmployeeCache.Where(c=> identities.Contains(c.Id.ToString("N")));

            var sb = new StringBuilder();
            bool isFirst = true;
            foreach (var employee in employees)
            {
                if (!isFirst)
                    sb.Append(",");
                isFirst = false;

                sb.Append(employee.Name);
            }

            return sb.ToString();
        }

        public static void UpdateTransitionHistory(ProcessInstance processInstance, string parameter)
        {
            var currentstate = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.CurrentState);
            var nextState = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);
            var command = WorkflowInit.Runtime.GetLocalizedCommandName(processInstance.ProcessId, processInstance.CurrentCommand);
            var isTimer = !string.IsNullOrEmpty(processInstance.ExecutedTimer);

            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            var document = dbcoll.Find(x=> x.Id == processInstance.ProcessId).FirstOrDefault();
            if (document == null)
                return;

            var historyItem =
                document.TransitionHistories.FirstOrDefault(
                    h => !h.TransitionTime.HasValue &&
                         h.InitialState == currentstate && h.DestinationState == nextState);

            if (historyItem == null)
            {
                historyItem = new DocumentTransitionHistory
                {
                    Id = Guid.NewGuid(),
                    AllowedToEmployeeNames = string.Empty,
                    DestinationState = nextState,
                    InitialState = currentstate
                };
                document.TransitionHistories.Add(historyItem);
            }

            historyItem.Command = !isTimer ? command : string.Format("Timer: {0}", processInstance.ExecutedTimer);
            historyItem.TransitionTime = DateTime.Now;

            if (string.IsNullOrWhiteSpace(processInstance.IdentityId))
            {
                historyItem.EmployeeId = null;
                historyItem.EmployeeName = string.Empty;
            }
            else
            {
                historyItem.EmployeeId = new Guid(processInstance.IdentityId);
                historyItem.EmployeeName = EmployeeHelper.EmployeeCache.First(c => c.Id == historyItem.EmployeeId).Name;
            }

            dbcoll.ReplaceOne(x => x.Id == document.Id, document, new UpdateOptions { IsUpsert = true });
        }

        internal static void DeleteEmptyPreHistory(Guid processId)
        {
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            var doc = dbcoll.Find(x => x.Id == processId).FirstOrDefault();
            if (doc != null)
            {
                doc.TransitionHistories.RemoveAll(dth => !dth.TransitionTime.HasValue);
                dbcoll.ReplaceOne(x => x.Id == doc.Id, doc, new UpdateOptions { IsUpsert = true });
            }
        }


        private static Dictionary<string, Action<ProcessInstance, string>> _actions = new Dictionary
            <string, Action<ProcessInstance, string>>
        {
            {"WriteTransitionHistory", WriteTransitionHistory},
            {"UpdateTransitionHistory", UpdateTransitionHistory}
        };

        private static Dictionary<string, Func<ProcessInstance, string, bool>> _conditions = new Dictionary<string, Func<ProcessInstance, string, bool>>
        {
          
        };

        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            if (_actions.ContainsKey(name))
            {
                _actions[name].Invoke(processInstance, actionParameter);
                return;
            }

            throw new NotImplementedException(string.Format("Action with name {0} not implemented", name));
        }

        public Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            if (_conditions.ContainsKey(name))
            {
                return _conditions[name].Invoke(processInstance, actionParameter);
            }

            throw new NotImplementedException(string.Format("Action condition with name {0} not implemented", name));
        }

        public Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public bool IsActionAsync(string name)
        {
            return false;
        }

        public bool IsConditionAsync(string name)
        {
            return false;
        }

        public List<string> GetActions()
        {
            return _actions.Keys.ToList();
        }

        public List<string> GetConditions()
        {
            return _conditions.Keys.ToList();
        }
    }
}
