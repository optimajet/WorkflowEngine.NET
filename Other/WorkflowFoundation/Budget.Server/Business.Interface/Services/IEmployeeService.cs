using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL;
using Employee = Budget2.Server.Business.Interface.DataContracts.Employee;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IEmployeeService
    {
        Guid? GetIdentityStructDivisionId(Guid trusteeId, Guid budgetId);

        Guid? GetIdentityStructDivisionId(Guid trusteeId, Guid budgetId, Budget2DataContext context);

        List<Guid> GetTrusteeInSameStructDivisionIds(Guid trusteeId, Guid budgetId);

        IEnumerable<Employee> GetEmployeesBySecurityTrusteeIdsForNotification(IEnumerable<Guid> securityTrusteeIds, Guid budgetId, bool addDeputies);

        IEnumerable<Guid> GetAllTrusteeInStructDivisionIds(Guid structDivisionId);

        IEnumerable<Employee> AddDeputies(IEnumerable<Employee> employees, Guid budgetId);

        CFO GetIdentityStructDivision(Guid trusteeId, Guid budgetId);

        CFO GetCfo(Guid cfoId);
    }
}
