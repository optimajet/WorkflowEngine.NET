using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;
using Microsoft.Practices.CompositeWeb;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Server.Security.AuthorizationValidators
{
    public class BillDemandWorkflowAuthorizationValidator : Budget2DataContextService, IAuthorizationValidator
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

            if (state.Type != WorkflowType.BillDemandWorkfow)
                return false;

            if ((commandType == WorkflowCommandType.Sighting) &&
                ((state == WorkflowState.BillDemandUPKZCuratorSighting) ||
                 (state == WorkflowState.BillLimitManagerSighting) ||
                 (state == WorkflowState.BillDemandUPKZCntrollerSighting) ||
                 (state == WorkflowState.BillDemandLimitExecutorSighting) ||
                 (state == WorkflowState.BillDemandUPKZHeadSighting) ||
                 (state == WorkflowState.BillDemandHeadInitiatorSighting)
                )
                )
                return true;

            if (((commandType == WorkflowCommandType.Denial) ||
                 (commandType == WorkflowCommandType.DenialByTechnicalCauses)) &&
                (
                    (state == WorkflowState.BillDemandUPKZCuratorSighting) ||
                    (state == WorkflowState.BillLimitManagerSighting) ||
                    (state == WorkflowState.BillDemandLimitExecutorSighting) ||
                     (state == WorkflowState.BillDemandUPKZHeadSighting) ||
                     (state == WorkflowState.BillDemandHeadInitiatorSighting))
                )
                return true;

            if ((commandType == WorkflowCommandType.Denial) &&
                ((state == WorkflowState.BillDemandPostingAccounting) ||
                 (state == WorkflowState.BillDemandUPKZCntrollerSighting) ||
                 (state == WorkflowState.BillDemandDraftForTechnicalDenial)))
                return true;

            if ((commandType == WorkflowCommandType.PostingAccounting) &&
                (state == WorkflowState.BillDemandPostingAccounting))
                return true;

            if ((commandType == WorkflowCommandType.Export) && (state == WorkflowState.BillDemandInAccountingWithExport))
                return true;

            if ((commandType == WorkflowCommandType.SetPaidStatus || commandType == WorkflowCommandType.SetDenialStatus) && (state == WorkflowState.BillDemandInAccountingWithExport))
                return true;

            if ((commandType == WorkflowCommandType.SetPaidStatus || commandType == WorkflowCommandType.Denial) && state == WorkflowState.BillDemandInitiatorConfirmation)
                return true;

            if ((commandType == WorkflowCommandType.StartProcessing) && (state == WorkflowState.BillDemandDraft || state == WorkflowState.BillDemandDraftForTechnicalDenial))
                return true;

            if (commandType == WorkflowCommandType.CheckStatus && state == WorkflowState.BillDemandOnPayment)
                return true;

            return false;
        }

        public bool IsCurrentUserAllowedToExecuteCommandInCurrentState(ServiceIdentity identity, WorkflowState state,
                                                                    Guid instanceId)
        {
            if (state == null)
            {
                return ValidateInitiator(identity, instanceId);
            }

            if (state.Type != WorkflowType.BillDemandWorkfow)
                return false;

            if (state == WorkflowState.BillDemandOnPayment)
                return true;

            if ((state == WorkflowState.BillDemandDraft) || (state == WorkflowState.BillDemandPostingAccounting) || state == WorkflowState.BillDemandDraftForTechnicalDenial || state == WorkflowState.BillDemandInitiatorConfirmation)
                return ValidateInitiator(identity, instanceId);

            if (state == WorkflowState.BillDemandUPKZCntrollerSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.Controller);

            if (state == WorkflowState.BillDemandUPKZCuratorSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.Curator);

            if (state == WorkflowState.BillDemandUPKZHeadSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.UPKZHead);

            if (state == WorkflowState.BillLimitManagerSighting)
                return ValidateLimitManager(identity, instanceId);

            if (state == WorkflowState.BillDemandInAccountingWithExport)
                return  AuthorizationService.IsInRole(identity.Id, BudgetRole.Accountant);

            if (state == WorkflowState.BillDemandLimitExecutorSighting)
                return ValidateLimitExecutor(identity, instanceId);

            if (state == WorkflowState.BillDemandHeadInitiatorSighting)
                return AuthorizationService.IsInRole(identity.Id, BudgetRole.DivisionHead) &&
                       ValidateInitiatorHead(identity, instanceId);

            return false;
        }

        private bool ValidateInitiatorHead(ServiceIdentity identity, Guid instanceId)
        {
             using (var scope = new TransactionScope())
             using (var context = CreateContext())
             {
                 var initiatorDivisionId =
                     context.BillDemands.Where(p => p.Id == instanceId).Select(
                         p => p.Author.Employees.First().StructDivisionId).First();
                 var identityDivisionId =
                     context.SecurityTrustees.Where(p => p.Id == identity.Id).Select(
                         p => p.Employees.First().StructDivisionId).First();
                 return initiatorDivisionId == identityDivisionId;
             }
        }


        private bool ValidateLimitManager(ServiceIdentity identity, Guid instanceId)
        {
            return CheckLimitSighting(identity, instanceId, SightingType.BillDemandLimitManagerSighting);
        }

        private bool CheckLimitSighting(ServiceIdentity identity, Guid instanceId, SightingType type)
        {
            var dlo = new DataLoadOptions();
            dlo.LoadWith<BillDemandDistribution>(p => p.Demand);
            dlo.LoadWith<Demand>(p => p.Limit);

            using (var scope = new TransactionScope())
            using (var context = CreateContext())
            {
                context.LoadOptions = dlo;

                var distributions = GetBillDemandDistributions(instanceId, identity.Id, type, context);

                if (distributions.Count() == 0)
                    return false;

                var sightings = GetSightings(instanceId, identity.Id, type, context);

                if (sightings.Count() == 0)
                    return true;

                return !distributions.TrueForAll(
                   p =>
                   p.DemandId.HasValue && p.Demand.LimitId.HasValue &&
                   sightings.FirstOrDefault(s => s.ItemId == p.Demand.LimitId)!=null);

            }
        }

        private List<WorkflowSighting> GetSightings (Guid instanceId, Guid initiatorId,  SightingType type, Budget2DataContext context)
        {
            return
                context.WorkflowSightings.Where(
                    p => p.SighterId == initiatorId && p.EntityId == instanceId && p.SightingType ==
                         type.Id).ToList();
            
        }

        private List<BillDemandDistribution> GetBillDemandDistributions (Guid instanceId, Guid initiatorId,  SightingType type, Budget2DataContext context)
        {
            if (type == SightingType.BillDemandLimitManagerSighting)
                return
                    context.BillDemandDistributions.Where(
                        p => p.DemandId.HasValue &&
                        p.BillDemandId == instanceId && p.Demand.LimitId.HasValue && p.Demand.Limit.ManagerId.HasValue &&
                        p.Demand.Limit.ManagerId.Value == initiatorId).ToList();
            else if (type == SightingType.BillDemandLimitExecutorSighting)
                return
                   context.BillDemandDistributions.Where(
                       p => p.DemandId.HasValue &&
                       p.BillDemandId == instanceId && p.Demand.LimitId.HasValue && p.Demand.Limit.ExecutorId.HasValue &&
                       p.Demand.Limit.ExecutorId.Value == initiatorId).ToList();
            else
            {
                throw new ArgumentException("Неизвестный тип параллельного согласования");
            }
        }

        private bool ValidateLimitExecutor(ServiceIdentity identity, Guid instanceId)
        {
            return CheckLimitSighting(identity, instanceId, SightingType.BillDemandLimitExecutorSighting);
        }

        private bool ValidateInitiator(ServiceIdentity identity, Guid instanceId)
        {
            using (var context = CreateContext())
            {
                var billDemand = context.BillDemands.SingleOrDefault(p => p.Id == instanceId);
                if (billDemand == null)
                    return false;

                if (billDemand.AuthorId == identity.Id)
                    return true;
            }

            return false;
        }
    }
}