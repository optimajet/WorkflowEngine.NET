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
    public class ActionProvider : IWorkflowActionProvider
    {
        private readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>> _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>> _asyncActions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();

        private readonly IDataServiceProvider _dataServiceProvider;
        
        public ActionProvider(IDataServiceProvider dataServiceProvider)
        {
            _dataServiceProvider = dataServiceProvider;
            //Register your actions in _actions and _asyncActions dictionaries
//            _actions.Add("MyAction", MyAction); //sync
//            _asyncActions.Add("MyAsyncAction", MyAsyncAction); //async

            //Register your conditions in _conditions and _asyncConditions dictionaries
//            _conditions.Add("MyCondition", MyCondition); //sync
//            _asyncConditions.Add("MyAsyncCondition", MyAsyncCondition); //async
        }

        private void MyAction(ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            //Execute your synchronous code here
        }

        private async Task MyAsyncAction(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //Execute your asynchronous code here. You can use await in your code.
        }

        private bool MyCondition(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            //Execute your code here
            return false;
        }

        private async Task<bool> MyAsyncCondition(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //Execute your asynchronous code here. You can use await in your code.
            return false;
        }

        #region Implementation of IWorkflowActionProvider

        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_actions.ContainsKey(name))
                _actions[name].Invoke(processInstance, runtime, actionParameter);
            else
                throw new NotImplementedException($"Action with name {name} isn't implemented");
        }

        public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncActions.ContainsKey(name))
                await _asyncActions[name].Invoke(processInstance, runtime, actionParameter, token);
            else
                throw new NotImplementedException($"Async Action with name {name} isn't implemented");
        }

        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_conditions.ContainsKey(name))
                return _conditions[name].Invoke(processInstance, runtime, actionParameter);

            throw new NotImplementedException($"Condition with name {name} isn't implemented");
        }

        public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncConditions.ContainsKey(name))
                return await _asyncConditions[name].Invoke(processInstance, runtime, actionParameter, token);

            throw new NotImplementedException($"Async Condition with name {name} isn't implemented");
        }

        public bool IsActionAsync(string name, string schemeCode)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name, string schemeCode)
        {
            return _asyncConditions.ContainsKey(name);
        }

        public List<string> GetActions(string schemeCode)
        {
            return _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions(string schemeCode)
        {
            return _conditions.Keys.Union(_asyncConditions.Keys).ToList();
        }

        #endregion
    }
}

