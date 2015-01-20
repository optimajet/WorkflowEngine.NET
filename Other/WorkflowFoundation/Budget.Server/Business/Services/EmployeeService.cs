using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Transactions;
using Budget2.DAL;
using Budget2.Server.Business.Interface.Services;
using Employee = Budget2.Server.Business.Interface.DataContracts.Employee;

namespace Budget2.Server.Business.Services
{
    public class EmployeeService : Budget2DataContextService, IEmployeeService 
    {
        public Guid? GetIdentityStructDivisionId(Guid trusteeId,Guid budgetId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    return context.Employees.Where(p => p.SecurityTrusteeId == trusteeId && p.BudgetId == budgetId && !p.IsDeleted).Select(
                        p => p.StructDivisionId).FirstOrDefault();

                    
                }
            }
        }

        public List<Guid> GetTrusteeInSameStructDivisionIds(Guid trusteeId, Guid budgetId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    var structDivisionId = context.Employees.Where(p => p.SecurityTrusteeId == trusteeId && p.BudgetId == budgetId && !p.IsDeleted).Select(
                        p => p.StructDivisionId).FirstOrDefault();

                    if (structDivisionId == null)
                        return new List<Guid>();

                    return context.Employees.Where(p => p.StructDivisionId == structDivisionId && !p.IsDeleted && p.SecurityTrusteeId.HasValue).Select(p=>p.SecurityTrusteeId.Value).Distinct().ToList();
                }
            }
        }

        public IEnumerable<Guid> GetAllTrusteeInStructDivisionIds(Guid structDivisionId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    return context.Employees.Where(p => p.StructDivisionId == structDivisionId && !p.IsDeleted && p.SecurityTrusteeId.HasValue).Select(p => p.SecurityTrusteeId.Value).Distinct().ToList();
                }
            }
        }

        public IEnumerable<Employee> GetEmployeesBySecurityTrusteeIdsForNotification(IEnumerable<Guid> securityTrusteeIds)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    return
                        context.Employees.Where(
                            empl => empl.SecurityTrusteeId != null && empl.EMail != null && empl.EMail != string.Empty && empl.IsSendWorkflowNotification && securityTrusteeIds.Contains(empl.SecurityTrusteeId.Value)).Select(
                                empl => new Employee() {Email = empl.EMail, Id = empl.Id, IdentityId = empl.SecurityTrusteeId.Value}).ToList();
                }
            }
        }
    }
}
