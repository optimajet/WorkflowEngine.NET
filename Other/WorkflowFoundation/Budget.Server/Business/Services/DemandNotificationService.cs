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

        public DemandNotificationService()
        {
            _senders.Add(WorkflowState.DemandInitiatorHeadSighting, SendOnInitiatorHeadSighting);

            //_senders.Add(WorkflowState.DemandAgreementInitiatorHeadSighting, SendOnInitiatorHeadSighting);

            _senders.Add(WorkflowState.DemandOPExpertSighting, SendOnOPExpertSighting);

            _senders.Add(WorkflowState.DemandAgreementOPExpertSighting, SendOnAgreementOPExpertSighting);

            _senders.Add(WorkflowState.DemandUPKZCuratorSighting, SendOnUPKZCuratorSighting);

            _senders.Add(WorkflowState.DemandUPKZHeadSighting, SendOnUPKZHeadSighting);

            _senders.Add(WorkflowState.DemandOPHeadSighting, SendOnOPHeadSighting);

           // _senders.Add(WorkflowState.DemandAgreementOPHeadSighting, SendOnAgreementOPHeadSighting);

            _senders.Add(WorkflowState.DemandRollbackRequested, SendOnRollbackRequested);
        }

        private void SendOnOPHeadSighting(Demand demand)
        {
            if (!demand.ExecutorStructId.HasValue)
                return;
            var identityIds = EmployeeService.GetAllTrusteeInStructDivisionIds(demand.ExecutorStructId.Value);
            var employees = SecurityEntityService.GetAllEmployeesInRole(identityIds, BudgetRole.DivisionHead, demand.BudgetVersion.BudgetId, true);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandOPHeadSighting);
        }

        //private void SendOnAgreementOPHeadSighting(Demand demand)
        //{
        //    if (!demand.AgreementCfo.HasValue)
        //        return;
        //    var identityIds = EmployeeService.GetAllTrusteeInStructDivisionIds(demand.AgreementCfo.Value);
        //    var employees = SecurityEntityService.GetAllEmployeesInRole(identityIds, BudgetRole.DivisionHead, demand.BudgetVersion.BudgetId, true);
        //    SendMailsToEmployee(employees, demand, WorkflowState.DemandAgreementOPHeadSighting);
        //}

        private void SendOnOPExpertSighting(Demand demand)
        {
            if (!demand.ExecutorStructId.HasValue)
                return;
            var identityIds = EmployeeService.GetAllTrusteeInStructDivisionIds(demand.ExecutorStructId.Value);
            var employees = SecurityEntityService.GetAllEmployeesInRole(identityIds, BudgetRole.Expert, demand.BudgetVersion.BudgetId, true);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandOPExpertSighting);
        }

        private void SendOnAgreementOPExpertSighting(Demand demand)
        {
            if (!demand.AgreementCfo.HasValue)
                return;
            var identityIds = EmployeeService.GetAllTrusteeInStructDivisionIds(demand.AgreementCfo.Value);
            var employees = EmployeeService.GetEmployeesBySecurityTrusteeIdsForNotification(identityIds, demand.BudgetVersion.BudgetId, true);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandAgreementOPExpertSighting);
        }

        private void SendOnInitiatorHeadSighting(Demand demand)
        {
            if (!demand.AuthorId.HasValue)
                return;
            var identityIds = SecurityEntityService.GetHeadIds(demand.AuthorId.Value, demand.BudgetVersion.BudgetId);
            var employees = EmployeeService.GetEmployeesBySecurityTrusteeIdsForNotification(identityIds,
                                                                                            demand.BudgetVersion.
                                                                                                BudgetId, true);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandInitiatorHeadSighting);
        }

        private void SendOnUPKZCuratorSighting(Demand demand)
        {
            if (!demand.InitiatorStructId.HasValue)
                return;
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.Curator, demand.BudgetVersion.BudgetId, true);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandUPKZCuratorSighting);
        }

        private void SendOnRollbackRequested(Demand demand)
        {
            if (!demand.InitiatorStructId.HasValue)
                return;
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.Curator, demand.BudgetVersion.BudgetId, true);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandRollbackRequested);
        }

        private void SendOnUPKZHeadSighting(Demand demand)
        {
            if (!demand.InitiatorStructId.HasValue)
                return;
            var employees = SecurityEntityService.GetAllEmployeesInRole(BudgetRole.UPKZHead, demand.BudgetVersion.BudgetId, true);
            SendMailsToEmployee(employees, demand, WorkflowState.DemandUPKZHeadSighting);
        }

        private void SendMailsToEmployee(IEnumerable<Employee> employees, Demand demand, WorkflowState state)
        {
            var hasErrors = false;
            var errors = new StringBuilder();
            var distinctemployees = employees;//.Distinct();
            foreach (var employee in distinctemployees)
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

        private void SendEmailToEmployee(Demand demand, Employee employee, WorkflowState state)
        {
            var parameters = GetDefaultParameters(demand);
            parameters.Add("$DEMANDLINK$", string.Format("{0}/Demand/?tid={1}", PublicPagesUrl, WorkflowTicketService.CreateTicket(employee.IdentityId, demand.Id, state.WorkflowStateName)));
            EmailService.SendEmail("DEMAND_NOTIFICATION", parameters, employee.Email);
        }

        private Dictionary<string, string> GetDefaultParameters(Demand demand)
        {
            return new Dictionary<string, string>()
                       {
                           {
                               "$DEMANDNUMBER$",
                               demand.Number.ToString(CultureInfo.CurrentCulture)
                               }
                       };
        }

        private string PublicPagesUrl
        {
            get { return ConfigurationManager.AppSettings["PublicPagesUrl"]; }
        }

        public void SendNotificationsForState(Guid demandUid, WorkflowState state)
        {
            var billDemand = DemandBusinessService.GetDemandWithBudgetVersion(demandUid);
            Action<Demand> action;
            if (_senders.TryGetValue(state, out action))
                action.Invoke(billDemand);
        }
    }
}