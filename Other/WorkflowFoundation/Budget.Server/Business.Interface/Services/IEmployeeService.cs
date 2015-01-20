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
        List<Guid> GetTrusteeInSameStructDivisionIds(Guid trusteeId, Guid budgetId);
        IEnumerable<Employee> GetEmployeesBySecurityTrusteeIdsForNotification(IEnumerable<Guid> securityTrusteeIds);
        IEnumerable<Guid> GetAllTrusteeInStructDivisionIds(Guid structDivisionId);
    }
}
