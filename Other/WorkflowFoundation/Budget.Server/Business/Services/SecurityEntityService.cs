using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Employee = Budget2.Server.Business.Interface.DataContracts.Employee;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Server.Business.Services
{
    public class SecurityEntityService : Budget2DataContextService, ISecurityEntityService
    {
        public IEnumerable<BudgetPermission> GetAlPermissionsForTrusteeAndworkflow (Guid trusteeId)
        {
            var selectedPermissionIds = BudgetPermission.AllPermissions.Select(p=>p.Id);

            if (selectedPermissionIds.Count() < 1)
                return new List<BudgetPermission> ();

            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var realPermissionIds =  context.SecurityRoleRecords.Where(
                        p => p.RecordType == 1 && selectedPermissionIds.Contains(p.PermissionId)
                            && p.SecurityRole.SecurityDescriptorRecords.Count(sdr => sdr.SecurityTrustee.Id == trusteeId || (sdr.SecurityTrustee.IsContainer && sdr.SecurityTrustee.SecurityGroups1.Count(t => t.TrusteeId == trusteeId) > 0)) > 0).Select(srr=>srr.PermissionId).ToList();
                    return
                        BudgetPermission.AllPermissions.Where(
                            p => realPermissionIds.Contains(p.Id));
                }
            }

        }

        public bool CheckTrusteeWithIdIsInRole(Guid trusteeId, BudgetRole role)
        {
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return context.SecurityDescriptorRecords.Count(
                        p =>
                        p.RoleId == role.Id &&
                        (p.TrusteeId == trusteeId ||
                         (p.SecurityTrustee.IsContainer &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => t.TrusteeId == trusteeId) > 0))) > 0;
                }
            }
        }

        public bool CheckThatSomebodyHasRole (BudgetRole role)
        {
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return context.SecurityDescriptorRecords.Count(
                        p =>
                        p.RoleId == role.Id &&
                        ((!p.SecurityTrustee.IsContainer && !p.SecurityTrustee.Employees.First().IsDeleted) ||
                         (p.SecurityTrustee.IsContainer &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => !t.SecurityTrustee.Employees.First().IsDeleted) >
                          0))) > 0;
                }

            }
        }

        public bool CheckThatSomebodyHasRoleInFilial(BudgetRole role, Guid filialId)
        {
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return context.SecurityDescriptorRecords.Count(
                        p =>
                        p.RoleId == role.Id &&
                        ((!p.SecurityTrustee.IsContainer && p.SecurityTrustee.Employees.Count(e=>!e.IsDeleted && e.CFO.FilialId.HasValue && e.CFO.FilialId == filialId) > 0) ||
                         (p.SecurityTrustee.IsContainer &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => t.SecurityTrustee.Employees.Count(e => !e.IsDeleted && e.CFO.FilialId.HasValue && e.CFO.FilialId == filialId) > 0) >
                          0))) > 0;
                }

            }
        }

        public bool CheckThatSomebodyHasRole(List<Guid> trusteeIds, BudgetRole role)
        {
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return context.SecurityDescriptorRecords.Count(
                        p =>
                        p.RoleId == role.Id &&
                        (trusteeIds.Contains(p.TrusteeId) ||
                         (p.SecurityTrustee.IsContainer &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => trusteeIds.Contains(t.TrusteeId)) > 0))) > 0;
                }
            }
        }

        public IEnumerable<Employee> GetAllEmployeesInRole(BudgetRole role)
        {
            Dictionary<Guid, Employee> emplRecord = new Dictionary<Guid, Employee>();
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var allEmployee = context.SecurityDescriptorRecords.Where(
                        p =>
                        p.RoleId == role.Id ||
                         (p.SecurityTrustee.IsContainer &&
                           p.SecurityTrustee.SecurityGroups1.Count(t => !t.SecurityTrustee.Employees.First().IsDeleted) > 0)).Select(
                              sdr => sdr.SecurityTrustee.Employees.Where(empl =>empl.SecurityTrusteeId != null && empl.EMail != null && empl.EMail != string.Empty && empl.IsSendWorkflowNotification));

                    foreach (var employees in allEmployee)
                    {
                        foreach (var employee in employees)
                        {
                            if (!emplRecord.ContainsKey(employee.Id))
                                emplRecord.Add(employee.Id, new Employee() { Email = employee.EMail, Id = employee.Id, IdentityId = employee.SecurityTrusteeId.Value});
                        }
                    }
                }
            }

            return emplRecord.Values;
        }

        public IEnumerable<Employee> GetAllEmployeesInRole(IEnumerable<Guid> trusteeIds, BudgetRole role)
        {
            Dictionary<Guid, Employee> emplRecord = new Dictionary<Guid, Employee>();
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var allEmployee = context.SecurityDescriptorRecords.Where(
                        p =>
                        p.RoleId == role.Id &&
                        (trusteeIds.Contains(p.TrusteeId) ||
                         (p.SecurityTrustee.IsContainer &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => trusteeIds.Contains(t.TrusteeId)) > 0))).Select(
                              sdr => sdr.SecurityTrustee.Employees.Where(empl => empl.SecurityTrusteeId != null &&  empl.EMail != null && empl.EMail != string.Empty && empl.IsSendWorkflowNotification));

                    foreach (var employees in allEmployee)
                    {
                        foreach (var employee in employees)
                        {
                            if (!emplRecord.ContainsKey(employee.Id))
                                emplRecord.Add(employee.Id, new Employee() { Email = employee.EMail, Id = employee.Id, IdentityId = employee.SecurityTrusteeId.Value });
                        }
                    }
                }
            }

            return emplRecord.Values;
        }
    }
}
