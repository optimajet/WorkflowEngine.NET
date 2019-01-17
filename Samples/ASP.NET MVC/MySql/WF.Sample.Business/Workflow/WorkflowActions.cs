using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.Business.Workflow
{
    public  class WorkflowActions : IWorkflowActionProvider
    {    
        public static void DeleteEmptyPreHistory(Guid processId)
        {
            WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().DeleteEmptyPreHistory(processId);
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

        public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
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

        public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
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
