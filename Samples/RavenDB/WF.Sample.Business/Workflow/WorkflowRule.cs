using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business.Models;
using WF.Sample.Business.Helpers;
using OptimaJet.Workflow.Core.Model;

namespace WF.Sample.Business.Workflow
{
    public class WorkflowRule : IWorkflowRuleProvider
    {
        private class RuleFunction
        {
            public Func<ProcessInstance, string, IEnumerable<string>> GetFunction { get; set; }

            public Func<ProcessInstance, string, string, bool> CheckFunction { get; set; }
        }

        private Dictionary<string, RuleFunction> _funcs =
            new Dictionary<string, RuleFunction>();


        public WorkflowRule()
        {
            _funcs.Add("IsDocumentAuthor", new RuleFunction(){CheckFunction = IsDocumentAuthor,GetFunction = GetDocumentAuthor});
            _funcs.Add("IsAuthorsBoss", new RuleFunction() { CheckFunction = IsAuthorsBoss, GetFunction = GetAuthorsBoss });
            _funcs.Add("IsDocumentController", new RuleFunction() { CheckFunction = IsDocumentController, GetFunction = GetDocumentController });
            _funcs.Add("CheckRole", new RuleFunction() { CheckFunction = CheckRole, GetFunction = GetInRole });
        }

        private IEnumerable<string> GetInRole(ProcessInstance processInstance, string parameter)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                return EmployeeHelper.EmployeeCache.Where(c => c.Roles.Any(r => r.Value == parameter)).Select(c => c.Id.ToString("N")).ToList();
            }
        }

        private bool CheckRole(ProcessInstance processInstance, string identityId, string parameter)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var emp = EmployeeHelper.EmployeeCache.Where(c=>c.Id == new Guid(identityId)).FirstOrDefault();
                return emp.Roles.Any(c => c.Value == parameter);
            }
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
            return GetAuthorsBoss(processInstance, parameter).Contains(identityId);
        }

        private IEnumerable<string> GetAuthorsBoss(ProcessInstance processInstance, string parameter)
        {
            var res = new List<string>();

            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var document = session.Load<Document>(processInstance.ProcessId);
                if (document == null)
                    return res;

                var sds = StructDivisionHelper.StructDivisionCache;
                var emps = EmployeeHelper.EmployeeCache;

                var author = emps.FirstOrDefault(c => c.Id == document.AuthorId);
                if (author == null)
                    return res;

                var currentSD = sds.FirstOrDefault(c => c.Id == author.StructDivisionId);
                while (currentSD != null)
                {
                    var headEmpIds = emps.Where(c => c.IsHead && c.StructDivisionId == currentSD.Id).Select(c => c.Id.ToString("N")).ToArray();
                    res.AddRange(headEmpIds);

                    if (currentSD.ParentId != null)
                        currentSD = sds.FirstOrDefault(c => c.Id == currentSD.ParentId);
                    else
                        currentSD = null;
                }

                return res.Distinct();
            }
        }

        private IEnumerable<string> GetDocumentController(ProcessInstance processInstance, string parameter)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var document = session.Load<Document>(processInstance.ProcessId);
                if (document == null || !document.EmloyeeControlerId.HasValue)
                    return new List<string> { };

                return new List<string> { document.EmloyeeControlerId.Value.ToString("N") };
            }
        }

        private IEnumerable<string> GetDocumentAuthor(ProcessInstance processInstance, string parameter)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var document = session.Load<Document>(processInstance.ProcessId);
                if (document == null)
                    return new List<string> { };
                return new List<string> { document.AuthorId.ToString("N") };
            }
        }

        private bool IsDocumentController(ProcessInstance processInstance, string identityId, string parameter)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var document = session.Load<Document>(processInstance.ProcessId);
                if (document == null)
                    return false;
                return document.EmloyeeControlerId.HasValue && document.EmloyeeControlerId.Value == new Guid(identityId);
            }
        }

        private bool IsDocumentAuthor(ProcessInstance processInstance, string identityId, string parameter)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var document = session.Load<Document>(processInstance.ProcessId);
                if (document == null)
                    return false;
                return document.AuthorId == new Guid(identityId);
            }
        }


    }
}
