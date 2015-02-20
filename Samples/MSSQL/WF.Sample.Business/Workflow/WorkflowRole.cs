using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;

namespace WF.Sample.Business.Workflow
{
    public class WorkflowRole : IWorkflowRoleProvider
    {
        public bool IsInRole(string identityId, string roleId)
        {
            using (var context = new DataModelDataContext())
            {
                return context.EmployeeRoles.Count(er => er.EmloyeeId == new Guid(identityId) && er.RoleId == new Guid(roleId)) > 0;
            }
        }

        public IEnumerable<string> GetAllInRole(string roleId)
        {
            using (var context = new DataModelDataContext())
            {
                return context.EmployeeRoles.Where(er => er.RoleId == new Guid(roleId)).Select(er=>er.EmloyeeId).ToList().Select(c=> c.ToString("N"));
            }
        }

    }
}
