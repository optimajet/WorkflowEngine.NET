using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Model;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.Business.Workflow
{
    public class WorkflowRule : IWorkflowRuleProvider
    {
        private class RuleFunction
        {
            public Func<ProcessInstance, string, IEnumerable<string>> GetFunction { get; set; }

            public Func<ProcessInstance, string, string, bool> CheckFunction { get; set; }
        }

        private readonly Dictionary<string, RuleFunction> _funcs =
            new Dictionary<string, RuleFunction>();

        private readonly IDataServiceProvider _dataServiceProvider;

        public WorkflowRule(IDataServiceProvider dataServiceProvider)
        {
            _dataServiceProvider = dataServiceProvider;
            _funcs.Add("IsDocumentAuthor", new RuleFunction(){CheckFunction = IsDocumentAuthor,GetFunction = GetDocumentAuthor});
            _funcs.Add("IsAuthorsBoss", new RuleFunction() { CheckFunction = IsAuthorsBoss, GetFunction = GetAuthorsBoss });
            _funcs.Add("IsDocumentController", new RuleFunction() { CheckFunction = IsDocumentController, GetFunction = GetDocumentController });
            _funcs.Add("CheckRole", new RuleFunction() { CheckFunction = CheckRole, GetFunction = GetInRole });
        }

        private IEnumerable<string> GetInRole(ProcessInstance processInstance, string parameter)
        {
            return _dataServiceProvider.Get<IEmployeeRepository>().GetInRole(parameter);
        }

        private bool CheckRole(ProcessInstance processInstance, string identityId, string parameter)
        {
            return _dataServiceProvider.Get<IEmployeeRepository>().CheckRole(new Guid(identityId), parameter);
        }

        public List<string> GetRules()
        {
            return _funcs.Select(c => c.Key).ToList();
        }


        public bool Check(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName, string parameter)
        {
            return _funcs.ContainsKey(ruleName) && _funcs[ruleName].CheckFunction.Invoke(processInstance, identityId, parameter);
        }

        public IEnumerable<string> GetIdentities(ProcessInstance processInstance, WorkflowRuntime runtime, string ruleName, string parameter)
        {
            return !_funcs.ContainsKey(ruleName)
                ? new List<string> {}
                : _funcs[ruleName].GetFunction.Invoke(processInstance, parameter);
        }

        private bool IsAuthorsBoss(ProcessInstance processInstance, string identityId, string parameter)
        {
            return _dataServiceProvider.Get<IDocumentRepository>().IsAuthorsBoss(processInstance.ProcessId, new Guid(identityId));
        }

        private IEnumerable<string> GetAuthorsBoss(ProcessInstance processInstance, string parameter)
        {
            return _dataServiceProvider.Get<IDocumentRepository>().GetAuthorsBoss(processInstance.ProcessId);
        }

        private IEnumerable<string> GetDocumentController(ProcessInstance processInstance, string parameter)
        {
            var document = _dataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId, false);

            if (document == null || !document.ManagerId.HasValue)
                return new List<string> { };

            return new List<string> { document.ManagerId.Value.ToString() };
        }

        private IEnumerable<string> GetDocumentAuthor(ProcessInstance processInstance, string parameter)
        {
            var document = _dataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
            if (document == null)
                return new List<string> { };
            return new List<string> { document.AuthorId.ToString() };
        }

        private bool IsDocumentController(ProcessInstance processInstance, string identityId, string parameter)
        {
            var document = _dataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
            if (document == null)
                return false;
            return document.ManagerId.HasValue && document.ManagerId.Value == new Guid(identityId);   
        }

        private bool IsDocumentAuthor(ProcessInstance processInstance, string identityId, string parameter)
        {
            var document = _dataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
            if (document == null)
                return false;
            return document.AuthorId == new Guid(identityId);   
        }


    }
}
