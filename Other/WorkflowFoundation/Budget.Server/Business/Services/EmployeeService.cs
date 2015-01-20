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
        public Guid? GetIdentityStructDivisionId(Guid trusteeId, Guid budgetId)
        {
            using (var context = this.CreateContext())
            {
                return GetIdentityStructDivisionId(trusteeId, budgetId, context);
            }

        }

        public Guid? GetIdentityStructDivisionId(Guid trusteeId, Guid budgetId, Budget2DataContext context)
        {
              return
                    context.Employees.Where(p => p.SecurityTrusteeId == trusteeId && p.BudgetId == budgetId && !p.IsDeleted).Select(p => p.StructDivisionId).FirstOrDefault();
        }

        public CFO GetCfo (Guid cfoId)
        {
            using (var context = this.CreateContext())
            {
                return
                    context.CFOs.Where(p => p.Id == cfoId).FirstOrDefault();


            }
        }

        public CFO GetIdentityStructDivision(Guid trusteeId, Guid budgetId)
        {
            using (var context = this.CreateContext())
            {
                return
                    context.Employees.Where(p => p.SecurityTrusteeId == trusteeId && p.BudgetId == budgetId && !p.IsDeleted).Select(p => p.CFO).FirstOrDefault();


            }

        }

        public List<Guid> GetTrusteeInSameStructDivisionIds(Guid trusteeId, Guid budgetId)
        {
            using (var context = this.CreateContext())
            {
                var structDivisionId =
                    context.Employees.Where(
                        p => p.SecurityTrusteeId == trusteeId && p.BudgetId == budgetId && !p.IsDeleted).Select(
                            p => p.StructDivisionId).FirstOrDefault();

                if (structDivisionId == null)
                    return new List<Guid>();

                return
                    context.Employees.Where(p => 
                        p.StructDivisionId == structDivisionId && !p.IsDeleted && p.SecurityTrusteeId.HasValue &&
                        p.SecurityTrustee.Enabled).Select(p => p.SecurityTrusteeId.Value).Distinct().ToList();
            }

        }

        public IEnumerable<Guid> GetAllTrusteeInStructDivisionIds(Guid structDivisionId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    return context.Employees.Where(p => p.StructDivisionId == structDivisionId && !p.IsDeleted && p.SecurityTrusteeId.HasValue && p.SecurityTrustee.Enabled).Select(p => p.SecurityTrusteeId.Value).Distinct().ToList();
                }
            }
        }

        public IEnumerable<Employee> GetEmployeesBySecurityTrusteeIdsForNotification(IEnumerable<Guid> securityTrusteeIds, Guid budgetId, bool addDeputies)
        {
            List<Employee> employees;
            
            using (var context = this.CreateContext())
            {
                employees =
                    context.Employees.Where(
                        empl =>
                        empl.BudgetId == budgetId && empl.SecurityTrusteeId != null && /*empl.EMail != null &&
                            .EMail != string.Empty && /*empl.IsSendWorkflowNotification &&*/ empl.SecurityTrustee.Enabled &&
                        securityTrusteeIds.Contains(empl.SecurityTrusteeId.Value)).Select(
                            empl =>
                            new Employee() { Email = empl.EMail, Id = empl.Id, IdentityId = empl.SecurityTrusteeId.Value, IsSendNotification = empl.IsSendWorkflowNotification})
                        .ToList();
            }

             if (!addDeputies)
                    return employees.Where(e=>e.IsSendNotification && !string.IsNullOrEmpty(e.Email));
             else
             {
                 return AddDeputies(employees, budgetId);
             }
        }

        public IEnumerable<Employee> AddDeputies(IEnumerable<Employee> employees, Guid budgetId)
        {
            var result = new Dictionary<Guid, Employee>();
            List<Budget2.DAL.Employee> depEmployees;
            IEnumerable<Guid> securityTrusteeIds = employees.Select(e => e.IdentityId).ToList();
            using (var context = this.CreateContext())
            {
                depEmployees = context.DeputyEmployees.Where(
                 de =>
                 de.StartDate <= DateTime.Today && de.EndDate >= DateTime.Today &&
                 securityTrusteeIds.Contains(de.Employee)
                 ).Select(
                     de =>
                     de.DeputySecurityTrustee.Employees.FirstOrDefault(
                         empl =>
                         empl.BudgetId == budgetId &&  /*empl.EMail!=null && empl.EMail!=string.Empty &&
                        empl.IsSendWorkflowNotification && */!empl.IsDeleted && empl.SecurityTrustee.Enabled)).ToList();
            }

            foreach (
                    var depEmployee in
                        depEmployees.Where(depEmployee => depEmployee != null && !result.ContainsKey(depEmployee.Id)))
            {
                result.Add(depEmployee.Id,
                           new Employee()
                           {
                               Email = depEmployee.EMail,
                               Id = depEmployee.Id,
                               IdentityId = depEmployee.SecurityTrusteeId.Value,
                               IsSendNotification = depEmployee.IsSendWorkflowNotification

                           });
            }

            foreach (var employee in employees.Where(employee => !result.ContainsKey(employee.Id)))
            {
                result.Add(employee.Id, employee);
            }

            return result.Values.Where(e => e.IsSendNotification && !string.IsNullOrEmpty(e.Email));
        }
    }


}

    

