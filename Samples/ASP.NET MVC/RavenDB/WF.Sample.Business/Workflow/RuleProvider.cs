using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Model;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.Business.Workflow
{
    public class RuleProvider : IWorkflowRuleProvider
    {
        private class RuleFunction
        {
            public Func<ProcessInstance, WorkflowRuntime, string, IEnumerable<string>> GetFunction { get; set; }

            public Func<ProcessInstance, WorkflowRuntime, string, string, bool> CheckFunction { get; set; }
        }

        private readonly Dictionary<string, RuleFunction> _rules = new Dictionary<string, RuleFunction>();

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

        public bool MyRuleCheck(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId,
            string parameter)
        {
            return false;
        }

        #region Implementation of IWorkflowRuleProvider

        public List<string> GetRules()
        {
            return _rules.Keys.ToList();
        }

        public bool Check(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName,
            string parameter)
        {
            if (_rules.ContainsKey(ruleName))
                return _rules[ruleName].CheckFunction(processInstance, runtime, identityId, parameter);
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetIdentities(ProcessInstance processInstance, WorkflowRuntime runtime,
            string ruleName, string parameter)
        {
            if (_rules.ContainsKey(ruleName))
                return _rules[ruleName].GetFunction(processInstance, runtime, parameter);
            throw new NotImplementedException();
        }

        #endregion
    }
}
