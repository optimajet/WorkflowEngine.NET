using System;
using System.Collections.Generic;
using System.Data.Linq;
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

        public ISecurityEntityService SecurityEntityService { get; set; }
    
        public IBillDemandBuinessService BillDemandBuinessService { get; set; }
        
        public List<WorkflowCommandType> AddAdditionalCommand(ServiceIdentity getCurrentIdentity, IEnumerable<ServiceIdentity> identities, WorkflowState currentState, Guid instanceUid, List<WorkflowCommandType> allowedOperations)
        {
            if (currentState == WorkflowState.DemandDraft || currentState == WorkflowState.DemandAgreed || currentState == WorkflowState.DemandRollbackRequested || currentState == null)
                return allowedOperations;

            if (ValidateInitiatorForRollback(getCurrentIdentity, instanceUid))
                allowedOperations.Add(WorkflowCommandType.Rollback);
            else if (!getCurrentIdentity.IsImpersonated)
            {
                var identity = identities.FirstOrDefault(i => ValidateInitiatorForRollback(i, instanceUid));
                    if (identity != null)
                    {
                        getCurrentIdentity.TryImpersonate(identity.Id);
                        allowedOperations.Add(WorkflowCommandType.Rollback);
                    }

            }

            return allowedOperations;
        }


        public bool IsCommandSupportsInState(WorkflowState state, WorkflowCommandType commandType, Guid instanceId)
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
                 (state == WorkflowState.DemandUPKZHeadSighting) ||
                 //(state == WorkflowState.DemandAgreementInitiatorHeadSighting) ||
                 //(state == WorkflowState.DemandAgreementOPHeadSighting) ||
                 (state == WorkflowState.DemandAgreementOPExpertSighting) || (state == WorkflowState.DemandRollbackRequested) 
                )
                )
                return true;

            if ((commandType == WorkflowCommandType.Denial) &&
               ((state == WorkflowState.DemandInitiatorHeadSighting) ||
                (state == WorkflowState.DemandOPExpertSighting) ||
                (state == WorkflowState.DemandOPHeadSighting) ||
                (state == WorkflowState.DemandUPKZCuratorSighting) ||
                (state == WorkflowState.DemandUPKZHeadSighting) ||
                //(state == WorkflowState.DemandAgreementInitiatorHeadSighting) ||
                // (state == WorkflowState.DemandAgreementOPHeadSighting) ||
                 (state == WorkflowState.DemandAgreementOPExpertSighting) || (state == WorkflowState.DemandRollbackRequested) 
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

            if (state == WorkflowState.DemandRollbackRequested)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.Curator);

            if (state == WorkflowState.DemandUPKZHeadSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.UPKZHead);

            if (state == WorkflowState.DemandOPExpertSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.Expert) && ValidateCurrentUserInOP(identity, instanceId);

            if (state == WorkflowState.DemandOPHeadSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.DivisionHead) && ValidateCurrentUserInOP(identity, instanceId);

            if (state == WorkflowState.DemandAgreementOPExpertSighting)
            {
                //http://pmtask.ru/redmine/issues/867
                //return AuthorizationService.IsInRole(identity.Id, BudgetRole.Expert) && ValidateCurrentUserInAgreementOP(identity, instanceId);
                return ValidateCurrentUserInAgreementOP(identity, instanceId);
            }

            //if (state == WorkflowState.DemandAgreementOPHeadSighting)
            //    return AuthorizationService.IsInRole(identity.Id, BudgetRole.DivisionHead) && ValidateCurrentUserInAgreementOP(identity, instanceId);

            if (state == WorkflowState.DemandInitiatorHeadSighting /*|| state == WorkflowState.DemandAgreementInitiatorHeadSighting*/)
               return  ValidateInitiatorHead(identity, instanceId);



            return false;
        }

        private bool ValidateInitiatorHead(ServiceIdentity identity, Guid instanceId)
        {
            using (var context = CreateContext())
            {
                var dlo = new DataLoadOptions();
                dlo.LoadWith<Demand>(d => d.BudgetVersion);
                context.LoadOptions = dlo;

                var demand = context.Demands.FirstOrDefault(
                    p =>
                    p.Id == instanceId && p.AuthorId.HasValue);

                if (demand == null)
                    return false;

                var heads = SecurityEntityService.GetHeadIds(demand.AuthorId.Value, demand.BudgetVersion.BudgetId);

                return heads.Contains(identity.Id);
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


        private bool ValidateCurrentUserInAgreementOP(ServiceIdentity identity, Guid instanceId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    var demand = context.Demands.FirstOrDefault(
                        p =>
                        p.Id == instanceId && p.AgreementCfo.HasValue);

                    if (demand == null)
                        return false;

                    Guid? identityDivisionId = GetIdentityDivisionId(context, identity, demand.BudgetVersion.BudgetId);

                    if (!identityDivisionId.HasValue)
                        return false;

                    return demand.AgreementCfo == identityDivisionId.Value;

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

        private bool ValidateInitiatorForRollback(ServiceIdentity identity, Guid instanceId)
        {
            using (var context = CreateContext())
            {
                var demand = context.Demands.SingleOrDefault(p => p.Id == instanceId && p.PlanningTypeId != 2);
                if (demand == null)
                    return false;

                return demand.AuthorId == identity.Id;
            }
        }
    }
}
