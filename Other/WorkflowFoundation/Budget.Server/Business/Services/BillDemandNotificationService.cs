using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Globalization;
using System.Text;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Microsoft.Practices.CompositeWeb;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Employee = Budget2.Server.Business.Interface.DataContracts.Employee;

namespace Budget2.Server.Business.Services
{
    public class BillDemandNotificationService : Budget2DataContextService, IBillDemandNotificationService
    {
        [ServiceDependency]
        public IEmployeeService EmployeeService { get; set; }


        [ServiceDependency]
        public IWorkflowTicketService WorkflowTicketService { get; set; }

        [ServiceDependency]
        public IBillDemandBuinessService BillDemandBuinessService { get; set; }

        [ServiceDependency]
        public IEmailService EmailService { get; set; }

        [ServiceDependency]
        public ISecurityEntityService SecurityEntityService { get; set; }

        public BillDemandNotificationService()
        {
            _checkings.Add(WorkflowState.BillDemandUPKZCuratorSighting, CheckOnInputUpkzCuratorSighting);
            _checkings.Add(WorkflowState.BillDemandUPKZHeadSighting, CheckOnInputUpkzHeadSighting);
            _checkings.Add(WorkflowState.BillDemandUPKZCntrollerSighting, CheckOnInputUpkzControllerSighting);
            _checkings.Add(WorkflowState.BillDemandLimitExecutorSighting, CheckOnInputLimitExecutorSighting);
            _checkings.Add(WorkflowState.BillLimitManagerSighting, CheckOnInputLimitManagerSighting);
            _checkings.Add(WorkflowState.BillDemandHeadInitiatorSighting, CheckOnInputHeadInitiatorSighting);
            _checkings.Add(WorkflowState.BillDemandInAccountingWithExport, CheckOnInputInAccountingWithExport);

            _senders.Add(WorkflowState.BillDemandUPKZCuratorSighting, SendOnInputUpkzCuratorSighting);
            _senders.Add(WorkflowState.BillDemandUPKZCntrollerSighting, SendOnInputUpkzControllerSighting);
            _senders.Add(WorkflowState.BillDemandHeadInitiatorSighting, SendOnInputHeadInitiatorSighting);
            _senders.Add(WorkflowState.BillDemandUPKZHeadSighting, SendOnInputUpkzHeadSighting);
            _senders.Add(WorkflowState.BillDemandLimitExecutorSighting, SendOnInputLimitExecutorSighting);
            _senders.Add(WorkflowState.BillLimitManagerSighting, SendOnInputLimitManagerSighting); 
        }

        private void CheckOnInputInAccountingWithExport(BillDemand billDemand)
        {
            if (!BillDemandBuinessService.IsBillDemandFilialSupportExport(billDemand.Id))
                return;

            if (!billDemand.FilialId.HasValue || !SecurityEntityService.CheckThatSomebodyHasRoleInFilial(BudgetRole.Accountant,billDemand.FilialId.Value))
            {
                var parameters = GetDefaultParameters(billDemand);
                EmailService.SendEmail("BD_NOT_FOUND_ACCOUNTANT", parameters);
            }
        }

        private IDictionary<WorkflowState, Action<BillDemand>> _checkings = new Dictionary<WorkflowState, Action<BillDemand>>();

        private IDictionary<WorkflowState, Action<BillDemand>> _senders = new Dictionary<WorkflowState, Action<BillDemand>>();

        private void SendOnInputUpkzCuratorSighting(BillDemand billDemand)
        {
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.Curator);
            SendMailsToEmployee(employees, billDemand, WorkflowState.BillDemandUPKZCuratorSighting);
        }

        private void SendOnInputLimitExecutorSighting(BillDemand billDemand)
        {
            var limits = GetLimits(billDemand, false);
            var executorIds = new List<Guid>();
            limits.Where(lim=>lim.ExecutorId.HasValue).ForEach(lim=>executorIds.Add(lim.ExecutorId.Value));
            var employees = EmployeeService.GetEmployeesBySecurityTrusteeIdsForNotification(executorIds);
            SendMailsToEmployee(employees, billDemand, WorkflowState.BillDemandLimitExecutorSighting);
        }

        private void SendOnInputLimitManagerSighting(BillDemand billDemand)
        {
            var limits = GetLimits(billDemand, false);
            var managerIds = new List<Guid>();
            limits.Where(lim => lim.ManagerId.HasValue).ForEach(lim => managerIds.Add(lim.ExecutorId.Value));
            var employees = EmployeeService.GetEmployeesBySecurityTrusteeIdsForNotification(managerIds);
            SendMailsToEmployee(employees, billDemand, WorkflowState.BillLimitManagerSighting);
        }

        private void SendOnInputUpkzHeadSighting(BillDemand billDemand)
        {
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.UPKZHead);
            SendMailsToEmployee(employees, billDemand, WorkflowState.BillDemandUPKZHeadSighting);
        }

        private void SendOnInputUpkzControllerSighting(BillDemand billDemand)
        {
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.Controller);
            SendMailsToEmployee(employees, billDemand, WorkflowState.BillDemandUPKZCntrollerSighting);
        }

        private void SendOnInputHeadInitiatorSighting(BillDemand billDemand)
        {
            if (!billDemand.AuthorId.HasValue)
                throw new InvalidOperationException("billDemand.AuthorId is null");

            var trusteeIds = EmployeeService.GetTrusteeInSameStructDivisionIds(billDemand.AuthorId.Value, billDemand.BudgetId);

            var employees = SecurityEntityService.GetAllEmployeesInRole(trusteeIds, BudgetRole.DivisionHead);

            SendMailsToEmployee(employees, billDemand, WorkflowState.BillDemandHeadInitiatorSighting);
        }

        private void SendMailsToEmployee(IEnumerable<Employee> employees, BillDemand billDemand, WorkflowState state)
        {
            bool hasErrors = false;
            StringBuilder errors = new StringBuilder();

            foreach (var employee in employees)
            {
                try
                {
                    SendEmailToEmployee(billDemand,employee,state );
                }
                catch (Exception ex)
                {
                    errors.AppendLine(ex.Message);
                    hasErrors = true;
                }
                
            }

            if (hasErrors)
                throw new InvalidOperationException(errors.ToString());
        }

        private void SendEmailToEmployee(BillDemand bd, Business.Interface.DataContracts.Employee employee, WorkflowState state)
        {
            var parameters = GetDefaultParameters(bd);
            parameters.Add("$BILLDEMANDLINK$",string.Format("{0}/BillDemand/?tid={1}",PublicPagesUrl,WorkflowTicketService.CreateTicket(employee.IdentityId,bd.Id,state.WorkflowStateName)));
            EmailService.SendEmail("BILL_DEMAND_NOTIFICATION",parameters,employee.Email);
        }

        private void CheckOnInputHeadInitiatorSighting(BillDemand billDemand)
        {
            if (!billDemand.AuthorId.HasValue)
                throw new InvalidOperationException("billDemand.AuthorId is null");

            var trusteeIds = EmployeeService.GetTrusteeInSameStructDivisionIds(billDemand.AuthorId.Value, billDemand.BudgetId);

            if (!SecurityEntityService.CheckThatSomebodyHasRole(trusteeIds, BudgetRole.DivisionHead))
            {
                var parameters = GetDefaultParameters(billDemand);
                parameters.Add("$INITIATOR$",billDemand.Author.Name);
                EmailService.SendEmail("BD_NOT_FOUND_INITIATOR_HEAD", parameters);
                
            }
        }

        private string PublicPagesUrl
        {
            get
            {
               return ConfigurationManager.AppSettings["PublicPagesUrl"];
            }
        }

        private void CheckOnInputUpkzCuratorSighting(BillDemand billDemand)
        {
            if (!SecurityEntityService.CheckThatSomebodyHasRole(BudgetRole.Curator))
            {
                EmailService.SendEmail("BD_NOT_FOUND_UPKZ_CURATOR", GetDefaultParameters(billDemand));
            }

        }

        private void CheckOnInputUpkzControllerSighting(BillDemand billDemand)
        {
            if (!SecurityEntityService.CheckThatSomebodyHasRole(BudgetRole.Controller))
            {
                EmailService.SendEmail("BD_NOT_FOUND_UPKZ_CONTROLLER", GetDefaultParameters(billDemand));
            }
        }

        private void CheckOnInputUpkzHeadSighting(BillDemand billDemand)
        {
            if (!SecurityEntityService.CheckThatSomebodyHasRole(BudgetRole.UPKZHead))
            {
                EmailService.SendEmail("BD_NOT_FOUND_UPKZ_HEAD", GetDefaultParameters(billDemand));
            }
        }

        private void CheckOnInputLimitManagerSighting(BillDemand billDemand)
        {

            List<Limit> limits = GetLimits(billDemand);
            foreach (var limit in limits)
            {
                if (!limit.ManagerId.HasValue ||
                    limit.Manager.Employees.Count(p => !p.IsDeleted && p.BudgetId == billDemand.BudgetId) <= 0)
                {
                    var parameters = GetDefaultParameters(billDemand);
                    parameters = AddLimitParameters(parameters, limit);
                    EmailService.SendEmail("BD_NOT_FOUND_LIMIT_MANADGER", parameters);
                }
            }

        }

        private List<Limit> GetLimits(BillDemand billDemand, bool loademployees = true)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    var dlo = new DataLoadOptions();
                    dlo.LoadWith<BillDemandDistribution>(p => p.Demand);
                    dlo.LoadWith<Demand>(p => p.Limit);
                    if (loademployees)
                    {
                        dlo.LoadWith<Limit>(p => p.Manager);
                        dlo.LoadWith<Limit>(p => p.Executor);
                        dlo.LoadWith<SecurityTrustee>(p => p.Employees);
                    }
                    context.LoadOptions = dlo;
                    context.DeferredLoadingEnabled = false;
                    return context.BillDemandDistributions.Where(p => p.BillDemandId == billDemand.Id).Select(
                        p => p.Demand.Limit).ToList();
                }
            }
        }

        private void CheckOnInputLimitExecutorSighting(BillDemand billDemand)
        {
            List<Limit> limits = GetLimits(billDemand);
            foreach (var limit in limits)
            {
                if (!limit.ExecutorId.HasValue || limit.Executor.Employees.Count(p => !p.IsDeleted && p.BudgetId == billDemand.BudgetId) <= 0)
                {
                    var parameters = GetDefaultParameters(billDemand);
                    parameters = AddLimitParameters(parameters, limit);
                    EmailService.SendEmail("BD_NOT_FOUND_LIMIT_EXECUTOR", parameters);
                }
            }
        }

        private Dictionary<string,string> GetDefaultParameters (BillDemand billDemand)
        {
            return new Dictionary<string, string>()
                       {
                           {"$BILLDEMANDNUMBER$",
                               billDemand.IdNumber.ToString(CultureInfo.CurrentCulture)},
                           {
                               "$BILLDEMANDDATE$",
                               billDemand.AllocationDate.HasValue
                                   ? billDemand.AllocationDate.Value.ToString(CultureInfo.CurrentCulture)
                                   : "-"
                               }
                       };
        }

        private Dictionary<string,string> AddLimitParameters (Dictionary<string,string> parameters, Limit limit)
        {
            parameters.Add("$LIMITCODE$", limit.Code);
            parameters.Add("$LIMITNAME$",limit.Name);
            return parameters;
        }

        public void SendNotificationsForState(Guid billDemandUid, WorkflowState state)
        {
            var billDemand = BillDemandBuinessService.GetBillDemand(billDemandUid);
            Action<BillDemand> action;
            if (_senders.TryGetValue(state, out action))
                action.Invoke(billDemand);
        }

        public void CheckAndSendMailForState(Guid billDemandUid, WorkflowState state)
        {
            var billDemand = BillDemandBuinessService.GetBillDemand(billDemandUid);
            Action<BillDemand> action;
            if (_checkings.TryGetValue(state, out action))
                action.Invoke(billDemand);
        }

        public void CheckAndSendMail(Guid billDemandUid)
        {
            var billDemand = BillDemandBuinessService.GetBillDemand(billDemandUid);

            foreach (var item in _checkings.Values)
                item.Invoke(billDemand);
        }

        public void SendNotification(Guid billDemandUid, WorkflowState state)
        {
            
        }
    }
}
