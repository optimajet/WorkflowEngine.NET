using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Microsoft.Practices.CompositeWeb;
using Employee = Budget2.Server.Business.Interface.DataContracts.Employee;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Server.Business.Services
{
    public class SecurityEntityService : Budget2DataContextService, ISecurityEntityService
    {
        [ServiceDependency]
        public IEmployeeService EmployeeService { get; set; }

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
                        p.RoleId == role.Id && p.SecurityTrustee.Enabled &&
                        (p.TrusteeId == trusteeId ||
                         (p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => t.SecurityTrustee.Enabled && t.TrusteeId == trusteeId) > 0))) > 0;
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
                        ((!p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled  && !p.SecurityTrustee.Employees.First().IsDeleted) ||
                         (p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => t.SecurityTrustee.Enabled && !t.SecurityTrustee.Employees.First().IsDeleted) >
                          0))) > 0;
                }

            }
        }

        public bool CheckThatSomebodyHasRoleInFilial(BudgetRole role, Guid filialId, Guid budgetId)
        {
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return context.SecurityDescriptorRecords.Count(
                        p =>
                        p.RoleId == role.Id &&
                        ((!p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled  && p.SecurityTrustee.Employees.Count(e=>!e.IsDeleted && e.CFO.FilialId.HasValue && e.CFO.FilialId == filialId && e.BudgetId == budgetId) > 0) ||
                         (p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => t.SecurityTrustee.Enabled && t.SecurityTrustee.Employees.Count(e => !e.IsDeleted && e.CFO.FilialId.HasValue && e.CFO.FilialId == filialId && e.BudgetId == budgetId) > 0) >
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
                        ((trusteeIds.Contains(p.TrusteeId) && p.SecurityTrustee.Enabled) ||
                         (p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled && p.SecurityTrustee.SecurityGroups1.Count(t => t.SecurityTrustee.Enabled &&  trusteeIds.Contains(t.TrusteeId)) > 0))) > 0;
                }
            }
        }

        public IEnumerable<Employee> GetAllEmployeesInRole(BudgetRole role, Guid budgetId, bool addDeputies)
        {
            Dictionary<Guid, Employee> emplRecord = new Dictionary<Guid, Employee>();
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var allEmployee = context.SecurityDescriptorRecords.Where(
                        p =>
                        (p.RoleId == role.Id && p.SecurityTrustee.Enabled) ||
                         (p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled && 
                           p.SecurityTrustee.SecurityGroups1.Count(t => t.SecurityTrustee.Enabled && !t.SecurityTrustee.Employees.First().IsDeleted) > 0)).Select(
                              sdr => sdr.SecurityTrustee.Employees.Where(empl =>empl.SecurityTrusteeId != null /*&& empl.EMail != null && empl.EMail != string.Empty && empl.IsSendWorkflowNotification*/));

                    foreach (var employees in allEmployee)
                    {
                        foreach (var employee in employees)
                        {
                            if (!emplRecord.ContainsKey(employee.Id))
                                emplRecord.Add(employee.Id, new Employee() { Email = employee.EMail, Id = employee.Id, IdentityId = employee.SecurityTrusteeId.Value, IsSendNotification = employee.IsSendWorkflowNotification});
                        }
                    }
                }
            }

            if (!addDeputies)
                return emplRecord.Values.Where(e=>e.IsSendNotification && !string.IsNullOrEmpty(e.Email));
            else
                return EmployeeService.AddDeputies(emplRecord.Values, budgetId);
        }





        public IEnumerable<Guid> GetHeadIds(Guid trusteeId, Guid budgetId)
        {
            var checkedStructDivision = new List<Guid>();

            var cfo = EmployeeService.GetIdentityStructDivision(trusteeId, budgetId);

            checkedStructDivision.Add(cfo.Id);

            var ids = GetAllSecurityTrusteeInRoleInStructDivision(cfo.Id, BudgetRole.DivisionHead).ToList();
            if (ids.Count() > 0)
                return ids;
            for (int i = 0; i < 10; i++)
            {
                if (!cfo.ParentId.HasValue || checkedStructDivision.Contains(cfo.ParentId.Value))
                {
                    return new List<Guid>(0);
                }
                checkedStructDivision.Add(cfo.ParentId.Value);
                ids = GetAllSecurityTrusteeInRoleInStructDivision(cfo.ParentId.Value, BudgetRole.DivisionHead).ToList();
                if (ids.Count() > 0)
                    return ids;
                cfo = EmployeeService.GetCfo(cfo.ParentId.Value);
            }

            return new List<Guid>(0);
        }

        public IEnumerable<Guid> GetHeadIdsByDivision(Guid structdivisionId, Guid budgetId)
        {
            var checkedStructDivision = new List<Guid>();

            var cfo = EmployeeService.GetCfo(structdivisionId);

            checkedStructDivision.Add(cfo.Id);

            var ids = GetAllSecurityTrusteeInRoleInStructDivision(cfo.Id, BudgetRole.DivisionHead).ToList();
            if (ids.Count() > 0)
                return ids;
            for (int i = 0; i < 10; i++)
            {
                if (!cfo.ParentId.HasValue || checkedStructDivision.Contains(cfo.ParentId.Value))
                {
                    return new List<Guid>(0);
                }
                checkedStructDivision.Add(cfo.ParentId.Value);
                ids = GetAllSecurityTrusteeInRoleInStructDivision(cfo.ParentId.Value, BudgetRole.DivisionHead).ToList();
                if (ids.Count() > 0)
                    return ids;
                cfo = EmployeeService.GetCfo(cfo.ParentId.Value);
            }

            return new List<Guid>(0);
        }

        public IEnumerable<Guid> GetAllSecurityTrusteeInRoleInStructDivision(Guid structDivisionId, BudgetRole role)
        {
            using (var context = CreateContext())
            {
               var ids =
                        context.Employees.Where(
                            e => e.SecurityTrusteeId.HasValue && 
                            !e.IsDeleted && e.StructDivisionId == structDivisionId && e.SecurityTrustee.Enabled &&
                            e.SecurityTrustee.vSecurityTrusteeRoles.Count(sr => sr.RoleId == role.Id) > 0).Select(e => e.SecurityTrusteeId.Value).ToList();

                    return ids;
            }
        }

        public IEnumerable<Employee> GetAllEmployeesInRole(IEnumerable<Guid> trusteeIds, BudgetRole role, Guid budgetId, bool addDeputies)
        {
            var emplRecord = new Dictionary<Guid, Employee>();
            using (var context = CreateContext())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var allEmployee = context.SecurityDescriptorRecords.Where(
                        p =>
                        p.RoleId == role.Id &&
                        ((trusteeIds.Contains(p.TrusteeId) && p.SecurityTrustee.Enabled) ||
                         (p.SecurityTrustee.IsContainer && p.SecurityTrustee.Enabled &&
                          p.SecurityTrustee.SecurityGroups1.Count(t => t.SecurityTrustee.Enabled && trusteeIds.Contains(t.TrusteeId)) > 0))).Select(
                              sdr => sdr.SecurityTrustee.Employees.Where(empl => empl.SecurityTrusteeId != null /*&&  empl.EMail != null && empl.EMail != string.Empty && empl.IsSendWorkflowNotification*/));

                    foreach (var employees in allEmployee)
                    {
                        foreach (var employee in employees)
                        {
                            if (!emplRecord.ContainsKey(employee.Id))
                                emplRecord.Add(employee.Id, new Employee() { Email = employee.EMail, Id = employee.Id, IdentityId = employee.SecurityTrusteeId.Value, IsSendNotification = employee.IsSendWorkflowNotification });
                        }
                    }
                }
            }

            if (!addDeputies)
                return emplRecord.Values.Where(e=>e.IsSendNotification && !string.IsNullOrEmpty(e.Email));
            else
                return EmployeeService.AddDeputies(emplRecord.Values, budgetId);
        }
    }
}
