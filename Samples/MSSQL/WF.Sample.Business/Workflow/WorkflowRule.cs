using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;
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

        private readonly Dictionary<string, RuleFunction> _funcs =
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
             using (var context = new DataModelDataContext())
             {
                 return
                     context.EmployeeRoles.Where(r => r.Role.Name == parameter).ToList()
                         .Select(r => r.EmloyeeId.ToString("N")).ToList();
             }
        }

        private bool CheckRole(ProcessInstance processInstance, string identityId, string parameter)
        {
             using (var context = new DataModelDataContext())
             {
                 return context.EmployeeRoles.Any(
                     r => r.EmloyeeId == new Guid(identityId) && r.Role.Name == parameter);
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
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processInstance.ProcessId);
                if (document == null)
                    return false;
                return context.vHeads.Count(h => h.Id == document.AuthorId && h.HeadId == new Guid(identityId)) > 0;
            }
        }

        private IEnumerable<string> GetAuthorsBoss(ProcessInstance processInstance, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processInstance.ProcessId);
                if (document == null)
                    return new List<string> {};


                return
                    context.vHeads.Where(h => h.Id == document.AuthorId)
                        .Select(h => h.HeadId)
                        .ToList()
                        .Select(c => c.ToString("N"));
            }
        }

        private IEnumerable<string> GetDocumentController(ProcessInstance processInstance, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processInstance.ProcessId);
                if (document == null || !document.EmloyeeControlerId.HasValue)
                    return new List<string> {};

                return new List<string> {document.EmloyeeControlerId.Value.ToString("N")};
            }
        }

        private IEnumerable<string> GetDocumentAuthor(ProcessInstance processInstance, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processInstance.ProcessId);
                if (document == null)
                    return new List<string> {};
                return new List<string> {document.AuthorId.ToString("N")};
            }
        }

        private bool IsDocumentController(ProcessInstance processInstance, string identityId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processInstance.ProcessId);
                if (document == null)
                    return false;
                return document.EmloyeeControlerId.HasValue && document.EmloyeeControlerId.Value == new Guid(identityId);
            }
        }

        private bool IsDocumentAuthor(ProcessInstance processInstance, string identityId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processInstance.ProcessId);
                if (document == null)
                    return false;
                return document.AuthorId == new Guid(identityId);
            }
        }


    }
}
