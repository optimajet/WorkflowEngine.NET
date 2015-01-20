using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Server.Security.AuthorizationValidators
{
    public class DemandWorkflowAuthorizationValidator : Budget2DataContextService, IAuthorizationValidator
    {
        public IAuthorizationService AuthorizationService { get; set; }

        public IEmployeeService EmployeeService { get; set; }

        public bool IsCommandSupportsInState(WorkflowState state, WorkflowCommandType commandType)
        {
            if (state == null)
            {
                if (commandType == WorkflowCommandType.StartProcessing)
                    return true;
                return false;
            }

            if (state.Type != WorkflowType.DemandWorkflow)
                return false;

            if ((commandType == WorkflowCommandType.Sighting) &&
                ((state == WorkflowState.DemandInitiatorHeadSighting) ||
                 (state == WorkflowState.DemandOPExpertSighting) ||
                 (state == WorkflowState.DemandOPHeadSighting) ||
                 (state == WorkflowState.DemandUPKZCuratorSighting) ||
                 (state == WorkflowState.DemandUPKZHeadSighting) 
                )
                )
                return true;

            if ((commandType == WorkflowCommandType.Denial) &&
               ((state == WorkflowState.DemandInitiatorHeadSighting) ||
                (state == WorkflowState.DemandOPExpertSighting) ||
                (state == WorkflowState.DemandOPHeadSighting) ||
                (state == WorkflowState.DemandUPKZCuratorSighting) ||
                (state == WorkflowState.DemandUPKZHeadSighting)
               )
               )
                return true;
            if ((commandType == WorkflowCommandType.StartProcessing) && (state == WorkflowState.DemandDraft))
                return true;

            return false;
        }

        public bool IsCurrentUserAllowedToExecuteCommandInCurrentState(ServiceIdentity identity, WorkflowState state, Guid instanceId)
        {
            if (state == null)
            {
                return ValidateInitiator(identity, instanceId);
            }

            if (state.Type != WorkflowType.DemandWorkflow)
                return false;

            if (state == WorkflowState.DemandDraft)
                return ValidateInitiator(identity, instanceId);

            if (state == WorkflowState.DemandUPKZCuratorSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.Curator);

            if (state == WorkflowState.DemandUPKZHeadSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.UPKZHead);

            if (state == WorkflowState.DemandOPExpertSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.Expert) && ValidateCurrentUserInOP(identity, instanceId);

            if (state == WorkflowState.DemandOPHeadSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.DivisionHead) && ValidateCurrentUserInOP(identity, instanceId);

            if (state == WorkflowState.DemandInitiatorHeadSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.DivisionHead) && ValidateInitiatorHead(identity, instanceId);

            return false;
        }

        private bool ValidateInitiatorHead(ServiceIdentity identity, Guid instanceId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    var demand = context.Demands.FirstOrDefault(
                        p =>
                        p.Id == instanceId && p.AuthorStructDivisionId.HasValue);

                    if (demand == null)
                        return false;

                    Guid? identityDivisionId = GetIdentityDivisionId(context, identity, demand.BudgetVersion.BudgetId);

                    if (!identityDivisionId.HasValue)
                        return false;

                    return demand.AuthorStructDivisionId == identityDivisionId.Value;
                }
            }
        }

        private bool ValidateCurrentUserInOP(ServiceIdentity identity, Guid instanceId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    var demand = context.Demands.FirstOrDefault(
                        p =>
                        p.Id == instanceId && p.ExecutorStructId.HasValue);

                    if (demand == null)
                        return false;

                    Guid? identityDivisionId = GetIdentityDivisionId(context, identity, demand.BudgetVersion.BudgetId);

                    if (!identityDivisionId.HasValue)
                        return false;

                    return demand.ExecutorStructId == identityDivisionId.Value;
                        
                }
            }
        }

        private Guid? GetIdentityDivisionId(Budget2DataContext context, ServiceIdentity identity, Guid budgetId)
        {
            var employee =
                context.Employees.FirstOrDefault(p => p.SecurityTrusteeId == identity.Id && p.BudgetId == budgetId);
            return employee == null ? null : employee.StructDivisionId;
        }

        private bool ValidateInitiator(ServiceIdentity identity, Guid instanceId)
        {
            using (var context = CreateContext())
            {
                var demand = context.Demands.SingleOrDefault(p => p.Id == instanceId);
                if (demand == null)
                    return false;

                return demand.AuthorId == identity.Id;
            }
        }
    }
}
