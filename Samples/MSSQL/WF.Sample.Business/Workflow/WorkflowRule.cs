using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;

namespace WF.Sample.Business.Workflow
{
    public class WorkflowRule : IWorkflowRuleProvider
    {
        private class RuleFunction
        {
            public Func<Guid, string, IEnumerable<string>> GetFunction { get; set; }

            public Func<Guid, string, string, bool> CheckFunction { get; set; }
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

        private IEnumerable<string> GetInRole(Guid processId, string parameter)
        {
             using (var context = new DataModelDataContext())
             {
                 return
                     context.EmployeeRoles.Where(r => r.Role.Name == parameter).ToList()
                         .Select(r => r.EmloyeeId.ToString("N")).ToList();
             }
        }

        private bool CheckRole(Guid processId, string identityId, string parameter)
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


        public bool Check(Guid processId, string identityId, string ruleName, string parameter)
        {
            //if (ruleName.StartsWith(RolePrefix, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    var roleName = ruleName.Replace(RolePrefix, "");
            //    using (var context = new DataModelDataContext())
            //    {
            //        return context.EmployeeRoles.Any(
            //            r => r.EmloyeeId == new Guid(identityId) && r.Role.Name == roleName);
            //    }
            //}
            return _funcs.ContainsKey(ruleName) && _funcs[ruleName].CheckFunction.Invoke(processId, identityId,parameter);
        }

        public IEnumerable<string> GetIdentities(Guid processId, string ruleName, string parameter)
        {
            //if (ruleName.StartsWith(RolePrefix, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    var roleName = ruleName.Replace(RolePrefix, "");
            //    using (var context = new DataModelDataContext())
            //    {
            //        return
            //            context.EmployeeRoles.Where(r => r.Role.Name == roleName).ToList()
            //                .Select(r => r.EmloyeeId.ToString("N")).ToList();
            //    }
            //}

            return !_funcs.ContainsKey(ruleName)
                ? new List<string> {}
                : _funcs[ruleName].GetFunction.Invoke(processId,parameter);
        }

        private bool IsAuthorsBoss(Guid processId, string identityId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return false;
                return context.vHeads.Count(h => h.Id == document.AuthorId && h.HeadId == new Guid(identityId)) > 0;
            }
        }

        private IEnumerable<string> GetAuthorsBoss(Guid processId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return new List<string> {};


                return
                    context.vHeads.Where(h => h.Id == document.AuthorId)
                        .Select(h => h.HeadId)
                        .ToList()
                        .Select(c => c.ToString("N"));
            }
        }

        private IEnumerable<string> GetDocumentController(Guid processId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null || !document.EmloyeeControlerId.HasValue)
                    return new List<string> {};

                return new List<string> {document.EmloyeeControlerId.Value.ToString("N")};
            }
        }

        private IEnumerable<string> GetDocumentAuthor(Guid processId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return new List<string> {};
                return new List<string> {document.AuthorId.ToString("N")};
            }
        }

        private bool IsDocumentController(Guid processId, string identityId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return false;
                return document.EmloyeeControlerId.HasValue && document.EmloyeeControlerId.Value == new Guid(identityId);
            }
        }

        private bool IsDocumentAuthor(Guid processId, string identityId, string parameter)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return false;
                return document.AuthorId == new Guid(identityId);
            }
        }


    }
}
