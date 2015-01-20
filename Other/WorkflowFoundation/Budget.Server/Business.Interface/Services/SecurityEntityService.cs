using System;
using System.Collections.Generic;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface ISecurityEntityService
    {
        bool CheckTrusteeWithIdIsInRole(Guid trusteeId, BudgetRole role);
        bool CheckThatSomebodyHasRole(BudgetRole role);
        bool CheckThatSomebodyHasRole(List<Guid> trusteeIds, BudgetRole role);
        bool CheckThatSomebodyHasRoleInFilial(BudgetRole role, Guid filialId);
        IEnumerable<Employee> GetAllEmployeesInRole(BudgetRole role);
        IEnumerable<Employee> GetAllEmployeesInRole(IEnumerable<Guid> trusteeIds, BudgetRole role);
        IEnumerable<BudgetPermission> GetAlPermissionsForTrusteeAndworkflow(Guid trusteeId);
    }
}
