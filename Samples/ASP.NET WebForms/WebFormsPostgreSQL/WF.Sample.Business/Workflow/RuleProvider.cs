using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Model;
using WF.Sample.Business.DataAccess;
using System.Threading.Tasks;
using System.Threading;


namespace WF.Sample.Business.Workflow
{
    public class RuleProvider : IWorkflowRuleProvider
    {
        private class RuleFunction
        {
            public Func<ProcessInstance, WorkflowRuntime, string, IEnumerable<string>> GetFunction { get; set; }

            public Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<IEnumerable<string>>> GetFunctionAsync { get; set; }

            public Func<ProcessInstance, WorkflowRuntime, string, string, bool> CheckFunction { get; set; }
            public Func<ProcessInstance, WorkflowRuntime, string, string, CancellationToken, Task<bool>> CheckFunctionAsync { get; set; }
        }

        private readonly Dictionary<string, RuleFunction> _rules = new Dictionary<string, RuleFunction>();
        private readonly Dictionary<string, RuleFunction> _asyncRules = new Dictionary<string, RuleFunction>();

        private readonly IDataServiceProvider _dataServiceProvider;

        public RuleProvider(IDataServiceProvider dataServiceProvider)
        {
            _dataServiceProvider = dataServiceProvider;
            //Register your rules in the _rules Dictionary
            //_rules.Add("MyRule", new RuleFunction { CheckFunction = MyRuleCheck, GetFunction = MyRuleGet });
        }

        public IEnumerable<string> MyRuleGet(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            return new List<string>();
        }

        public async Task<IEnumerable<string>> MyRuleGetAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            return new List<string>();
        }

        public bool MyRuleCheck(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string parameter)
        {
            return false;
        }

        public async Task<bool> MyRuleCheckAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string parameter)
        {
            return false;
        }
        #region Implementation of IWorkflowRuleProvider
        
        public List<string> GetRules(string schemeCode, NamesSearchType namesSearchType)
        {
            return _rules.Keys.Union(_asyncRules.Keys).ToList();
        }

        public List<string> GetActors(string schemeCode)
        {
            return new List<string>();
        }

        public bool Check(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName, string parameter)
        {
            if (_rules.ContainsKey(ruleName))
                return _rules[ruleName].CheckFunction(processInstance, runtime, identityId, parameter);
            throw new NotImplementedException();
        }

        public virtual async Task<bool> CheckAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName, string parameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncRules.ContainsKey(ruleName))
                return await _asyncRules[ruleName].CheckFunctionAsync(processInstance, runtime, identityId, parameter, token).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetIdentities(ProcessInstance processInstance, WorkflowRuntime runtime,string ruleName, string parameter)
        {
            if (_rules.ContainsKey(ruleName))
                return _rules[ruleName].GetFunction(processInstance, runtime, parameter);
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<string>> GetIdentitiesAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string ruleName, string parameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncRules.ContainsKey(ruleName))
                return await _asyncRules[ruleName].GetFunctionAsync(processInstance, runtime, parameter, token).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        public bool IsCheckAsync(string ruleName, string schemeCode)
        {
            return _asyncRules.ContainsKey(ruleName);
        }

        public bool IsGetIdentitiesAsync(string ruleName, string schemeCode)
        {
            return _asyncRules.ContainsKey(ruleName);
        }

        #endregion
    }
}
