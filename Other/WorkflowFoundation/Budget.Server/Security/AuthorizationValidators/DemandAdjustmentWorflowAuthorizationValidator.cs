using System;
using System.Linq;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Server.Security.AuthorizationValidators
{
    public class DemandAdjustmentWorflowAuthorizationValidator : Budget2DataContextService, IAuthorizationValidator
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

            if (state.Type != WorkflowType.DemandAdjustmentWorkflow)
                return false;

            if (commandType == WorkflowCommandType.PostingAccounting || commandType == WorkflowCommandType.DenialByTechnicalCauses)
                return false;

            if ((state == WorkflowState.DemandAdjustmentAgreed) || (state == WorkflowState.DemandAdjustmentArchived))
                return false;

            if (commandType == WorkflowCommandType.StartProcessing && state == WorkflowState.DemandAdjustmentDraft)
                return true;

            if ((commandType == WorkflowCommandType.Sighting || commandType == WorkflowCommandType.Denial) && state != WorkflowState.DemandAdjustmentDraft)
                return true;

            return false;
        }

        public bool IsCurrentUserAllowedToExecuteCommandInCurrentState(ServiceIdentity identity, WorkflowState state, Guid instanceId)
        {
            if (state == null)
            {
                return ValidateInitiator(identity, instanceId);
            }

            if (state.Type != WorkflowType.DemandAdjustmentWorkflow)
                return false;

            if (state == WorkflowState.DemandAdjustmentDraft)
                return ValidateInitiator(identity, instanceId);

            if (state == WorkflowState.DemandAdjustmentSourceDemandLimitManagerSighting)
                return ValidateSourceDemandLimitManager(identity, instanceId);

            if (state == WorkflowState.DemandAdjustmentSourceDemandLimitExecutorSighting)
                return ValidateSourceDemandLimitExecutor(identity, instanceId);

            if (state == WorkflowState.DemandAdjustmentTargetDemandLimitExecutorSighting)
                return ValidateTargetDemandLimitExecutor(identity, instanceId);

            if (state == WorkflowState.DemandAdjustmentTargetDemandLimitManagerSighting)
                return ValidateTargetDemandLimitManager(identity, instanceId);

            if (state == WorkflowState.DemandAdjustmentUPKZCuratorSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.Curator);

            if (state == WorkflowState.DemandAdjustmentUPKZHeadSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.UPKZHead);

            return false;
        }

        private bool ValidateTargetDemandLimitManager(ServiceIdentity identity, Guid instanceId)
        {

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                   return context.DemandAdjustments.Count(
                        p =>
                        p.Id == instanceId && p.TargetDemandId.HasValue &&
                        p.TargetDemand.LimitId.HasValue && p.TargetDemand.Limit.ManagerId == identity.Id) == 1;
                }
            }
        }

        private bool ValidateTargetDemandLimitExecutor(ServiceIdentity identity, Guid instanceId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    return context.DemandAdjustments.Count(
                        p =>
                        p.Id == instanceId && p.TargetDemandId.HasValue  &&
                        p.TargetDemand.LimitId.HasValue && p.TargetDemand.Limit.ExecutorId == identity.Id) == 1;
                }
            }
        }

        private bool ValidateSourceDemandLimitExecutor(ServiceIdentity identity, Guid instanceId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    return context.DemandAdjustments.Count(
                        p =>
                        p.Id == instanceId && p.SourceDemandId.HasValue && 
                        p.SourceDemand.LimitId.HasValue && p.SourceDemand.Limit.ExecutorId == identity.Id) == 1;
                }
            }
        }

        private bool ValidateSourceDemandLimitManager(ServiceIdentity identity, Guid instanceId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    return context.DemandAdjustments.Count(
                        p =>
                        p.Id == instanceId && p.SourceDemandId.HasValue && 
                        p.SourceDemand.LimitId.HasValue && p.SourceDemand.Limit.ManagerId == identity.Id) == 1;
                }
            }
        }

        private bool ValidateInitiator(ServiceIdentity identity, Guid instanceId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    return context.DemandAdjustments.Count(p => p.Id == instanceId && p.CreatorId == identity.Id) == 1;
                }
            }
        }
    }
}
