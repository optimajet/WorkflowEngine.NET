using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace WF.Sample.Business.Workflow
{
    public  class WorkflowActions : IWorkflowActionProvider
    {
        public static bool CheckDocumentHasController(ProcessInstance processInstance, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                return context.Documents.Count(d => d.Id == processInstance.ProcessId && d.EmloyeeControlerId.HasValue) > 0;
            }
        }

        public static bool CheckDocumentsAuthorIsBoss(ProcessInstance processInstance, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                return context.Documents.Count(d => d.Id == processInstance.ProcessId && d.Employee1.IsHead) > 0;
            }
        }

        public static bool CheckBigBossMustSight(ProcessInstance processInstance, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                return context.Documents.Count(d => d.Id == processInstance.ProcessId && d.Sum > 100) > 0;
            }
        }

        public static void WriteTransitionHistory(ProcessInstance processInstance, string parameter)
        {
            if (processInstance.IdentityIds == null)
                return;

            var currentstate = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.CurrentState);

            var nextState = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);

            var command = WorkflowInit.Runtime.GetLocalizedCommandName(processInstance.ProcessId, processInstance.CurrentCommand);

            using (var context = new DataModelDataContext())
            {
                GetEmployeesString(processInstance.IdentityIds, context);

                var historyItem = new DocumentTransitionHistory
                                      {
                                          Id = Guid.NewGuid(),
                                          AllowedToEmployeeNames = GetEmployeesString(processInstance.IdentityIds, context),
                                          DestinationState = nextState,
                                          DocumentId = processInstance.ProcessId,
                                          InitialState = currentstate,
                                          Command = command
                                      };
                context.DocumentTransitionHistories.InsertOnSubmit(historyItem);
                context.SubmitChanges();
            }
        }

        private static string GetEmployeesString(IEnumerable<string> identities, DataModelDataContext context)
        {
            var identitiesGuid = identities.Select(c => new Guid(c));

            var employees = context.Employees.Where(e => identitiesGuid.Contains(e.Id)).ToList();

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

            using (var context = new DataModelDataContext())
            {
                var historyItem =
                    context.DocumentTransitionHistories.FirstOrDefault(
                        h => h.DocumentId == processInstance.ProcessId && !h.TransitionTime.HasValue &&
                             h.InitialState == currentstate && h.DestinationState == nextState);

                if (historyItem == null)
                {
                    historyItem = new DocumentTransitionHistory
                    {
                        Id = Guid.NewGuid(),
                        AllowedToEmployeeNames = string.Empty,
                        DestinationState = nextState,
                        DocumentId = processInstance.ProcessId,
                        InitialState = currentstate
                    };

                    context.DocumentTransitionHistories.InsertOnSubmit(historyItem);

                }

                historyItem.Command = !isTimer ? command : string.Format("Timer: {0}",processInstance.ExecutedTimer);
                historyItem.TransitionTime = DateTime.Now;

                if (string.IsNullOrWhiteSpace(processInstance.IdentityId))
                    historyItem.EmployeeId = null;
                else
                    historyItem.EmployeeId = new Guid(processInstance.IdentityId);

                context.SubmitChanges();
            }
        }

        internal static void DeleteEmptyPreHistory(Guid processId)
        {
            using (var context = new DataModelDataContext())
            {
                var existingNotUsedItems =
                    context.DocumentTransitionHistories.Where(
                        dth =>
                        dth.DocumentId == processId && !dth.TransitionTime.HasValue).ToList();

                context.DocumentTransitionHistories.DeleteAllOnSubmit(existingNotUsedItems);
                context.SubmitChanges();
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
            {"CheckDocumentHasController",CheckDocumentHasController},
            {"CheckDocumentsAuthorIsBoss",CheckDocumentsAuthorIsBoss},
            {"CheckBigBossMustSight",CheckBigBossMustSight}
        };

        public void ExecuteAction(string name, ProcessInstance processInstance, string actionParameter)
        {
            if (_actions.ContainsKey(name))
            {
                _actions[name].Invoke(processInstance, actionParameter);
                return;
            }

            throw new NotImplementedException(string.Format("Action with name {0} not implemented", name));
        }

        public bool ExecuteCondition(string name, ProcessInstance processInstance, string actionParameter)
        {
            if (_conditions.ContainsKey(name))
            {
                return _conditions[name].Invoke(processInstance, actionParameter);
            }

            throw new NotImplementedException(string.Format("Action condition with name {0} not implemented", name));
        }

        public List<string> GetActions()
        {
            return _actions.Keys.Concat(_conditions.Keys).ToList();
        }
    }
}
