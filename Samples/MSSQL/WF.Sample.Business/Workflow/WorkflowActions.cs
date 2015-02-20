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
      
        public static string GetEmployeesString(IEnumerable<string> identities, DataModelDataContext context)
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

     
        public static void DeleteEmptyPreHistory(Guid processId)
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

        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
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
