using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Microsoft.Practices.CompositeWeb;
using Employee = Budget2.Server.Business.Interface.DataContracts.Employee;

namespace Budget2.Server.Business.Services
{
    public class DemandNotificationService : IDemandNotificationService
    { 
        [ServiceDependency]
        public IEmployeeService EmployeeService { get; set; }

        [ServiceDependency]
        public IWorkflowTicketService WorkflowTicketService { get; set; }

        [ServiceDependency]
        public IEmailService EmailService { get; set; }

        [ServiceDependency]
        public ISecurityEntityService SecurityEntityService { get; set; }

        [ServiceDependency]
        public IDemandBusinessService DemandBusinessService { get; set; }
        
        private IDictionary<WorkflowState, Action<Demand>> _senders = new Dictionary<WorkflowState, Action<Demand>>();

        public DemandNotificationService ()
        {
            _senders.Add(WorkflowState.DemandInitiatorHeadSighting,SendOnInitiatorHeadSighting);
            _senders.Add(WorkflowState.DemandOPExpertSighting,SendOnOPExpertSighting);
            _senders.Add(WorkflowState.DemandUPKZCuratorSighting,SendOnUPKZCuratorSighting);
            _senders.Add(WorkflowState.DemandUPKZHeadSighting,SendOnUPKZHeadSighting);
            _senders.Add(WorkflowState.DemandOPHeadSighting, SendOnOPHeadSighting);
        }

      

        private void SendOnOPHeadSighting (Demand demand)
        {
           if (!demand.ExecutorStructId.HasValue)
                return;
            var identityIds = EmployeeService.GetAllTrusteeInStructDivisionIds(demand.ExecutorStructId.Value);
            var employees = SecurityEntityService.GetAllEmployeesInRole(identityIds, BudgetRole.DivisionHead);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandOPHeadSighting);
        }

        private void SendOnOPExpertSighting(Demand demand)
        {
            if (!demand.ExecutorStructId.HasValue)
                return;
            var identityIds = EmployeeService.GetAllTrusteeInStructDivisionIds(demand.ExecutorStructId.Value);
            var employees = SecurityEntityService.GetAllEmployeesInRole(identityIds, BudgetRole.Expert);
            SendMailsToEmployee(employees,demand,WorkflowState.DemandOPExpertSighting);
        }

        private void SendOnInitiatorHeadSighting(Demand demand)
        {
            if (!demand.InitiatorStructId.HasValue)
                return;
            var identityIds = EmployeeService.GetAllTrusteeInStructDivisionIds(demand.InitiatorStructId.Value);
            var employees = SecurityEntityService.GetAllEmployeesInRole(identityIds, BudgetRole.DivisionHead);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandInitiatorHeadSighting);
        }

        private void SendOnUPKZCuratorSighting(Demand demand)
        {
            if (!demand.InitiatorStructId.HasValue)
                return;
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.Curator);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandUPKZCuratorSighting);
        }

        private void SendOnUPKZHeadSighting(Demand demand)
        {
             if (!demand.InitiatorStructId.HasValue)
                return;
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.UPKZHead);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandUPKZHeadSighting);
        }

        private void SendMailsToEmployee(IEnumerable<Employee> employees, Demand demand, WorkflowState state)
        {
            bool hasErrors = false;
            StringBuilder errors = new StringBuilder();

            foreach (var employee in employees)
            {
                try
                {
                    SendEmailToEmployee(demand, employee, state);
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

        private void SendEmailToEmployee(Demand demand,  Employee employee, WorkflowState state)
        {
            var parameters = GetDefaultParameters(demand);
            parameters.Add("$DEMANDLINK$", string.Format("{0}/Demand/?tid={1}", PublicPagesUrl, WorkflowTicketService.CreateTicket(employee.IdentityId, demand.Id, state.WorkflowStateName)));
            EmailService.SendEmail("DEMAND_NOTIFICATION", parameters, employee.Email);
        }

        private Dictionary<string, string> GetDefaultParameters(Demand demand)
        {
            return new Dictionary<string, string>()
                       {
                           {"$DEMANDNUMBER$",
                               demand.Number.ToString(CultureInfo.CurrentCulture)}
                       };
        }

        private string PublicPagesUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["PublicPagesUrl"];
            }
        }

        public void SendNotificationsForState(Guid demandUid, WorkflowState state)
        {
            var billDemand = DemandBusinessService.GetDemand(demandUid);
            Action<Demand> action;
            if (_senders.TryGetValue(state, out action))
                action.Invoke(billDemand);
        }


    }
}
