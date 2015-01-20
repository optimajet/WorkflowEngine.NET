using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface;
using Budget2.Server.Business.Interface.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Microsoft.Practices.CompositeWeb;

namespace Budget2.Server.Business.Services
{
    public class BillDemandBusinessService : Budget2DataContextService, IBillDemandBuinessService
    {
      
        [ServiceDependency]
        public ISecurityEntityService SecurityEntityService { get; set; }

        [ServiceDependency]
        public IWorkflowStateService WorkflowStateService { get; set; }

        public void CreateBillDemandPreHistory(Guid billDemandUid, WorkflowState state)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var existingNotUsedItems =
                        context.BillDemandTransitionHistories.Where(
                            bdth => bdth.BillDemandId == billDemandUid && !bdth.TransitionTime.HasValue).ToList();

                    context.BillDemandTransitionHistories.DeleteAllOnSubmit(existingNotUsedItems);

                    Guid? expectedInitiatorId = GetInitiatorId(billDemandUid, context);

                    WritePreHistory(billDemandUid, context, WorkflowState.BillDemandDraft,
                                    WorkflowState.BillDemandUPKZCntrollerSighting, expectedInitiatorId, state);

                    if (this.CheckInitiatorHeadMustSign(billDemandUid))
                    {
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandUPKZCntrollerSighting,
                                        WorkflowState.BillDemandHeadInitiatorSighting, null, state);
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandHeadInitiatorSighting,
                                        WorkflowState.BillDemandLimitExecutorSighting, null, state);
                    }
                    else
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandUPKZCntrollerSighting,
                                        WorkflowState.BillDemandLimitExecutorSighting, null, state);

                    var executors = GetLimitsExecutors(context, billDemandUid);

                    if (executors.Count > 0)
                    {
                        foreach (var executorId in executors)
                        {
                            WritePreHistory(billDemandUid, context, WorkflowState.BillDemandLimitExecutorSighting,
                                            WorkflowState.BillLimitManagerSighting, executorId, state);
                        }
                    }
                    else
                    {
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandLimitExecutorSighting,
                                        WorkflowState.BillLimitManagerSighting, null, state);
                    }

                    var managers = GetLimitManagers(context, billDemandUid);

                    if (managers.Count > 0)
                    {
                        foreach (var managerId in managers)
                        {
                            WritePreHistory(billDemandUid, context, WorkflowState.BillLimitManagerSighting,
                                            WorkflowState.BillDemandUPKZCuratorSighting, managerId, state);
                        }
                    }
                    else
                    {
                        WritePreHistory(billDemandUid, context, WorkflowState.BillLimitManagerSighting,
                                        WorkflowState.BillDemandUPKZCuratorSighting, null, state);
                    }

                    bool uPKZHeadMustSight = CheckUPKZHeadMustSight(billDemandUid); 
                    
                    var isBillDemandFilialSupportExport = this.IsBillDemandFilialSupportExport(billDemandUid);

                    if (uPKZHeadMustSight)
                    {
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandUPKZCuratorSighting,
                                        WorkflowState.BillDemandUPKZHeadSighting, null, state);
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandUPKZHeadSighting,
                                        isBillDemandFilialSupportExport
                                            ? WorkflowState.BillDemandPostingAccounting
                                            : WorkflowState.BillDemandInitiatorConfirmation, null, state);
                    }
                    else
                    {
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandUPKZCuratorSighting,
                                        WorkflowState.BillDemandPostingAccounting, null, state);
                    }

                    if (isBillDemandFilialSupportExport)
                    {
                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandPostingAccounting,
                                    WorkflowState.BillDemandInAccountingWithExport, expectedInitiatorId, state);

                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandInAccountingWithExport,
                                   WorkflowState.BillDemandOnPayment, null, WorkflowCommand.Export, state); 
                    }
                    else
                    {

                        WritePreHistory(billDemandUid, context, WorkflowState.BillDemandInitiatorConfirmation,
                                    WorkflowState.BillDemandPaid, expectedInitiatorId, state);
                    }

                 

                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public void SetAllocationDate(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    BillDemand billDemand = GetBillDemand(context, billDemandUid);
                    billDemand.AllocationDate = DateTime.Now;
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public void SetTransferDate(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    BillDemand billDemand = GetBillDemand(context, billDemandUid);
                    billDemand.TransferDate = DateTime.Now;
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public void SetTransferDateAndDateOfPerformance(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    BillDemand billDemand = GetBillDemand(context, billDemandUid);
                    billDemand.TransferDate = DateTime.Now;
                    billDemand.DateOfPerformance = DateTime.Now;
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public void SetExternalParameters(Guid billDemandUid, BillDemandExternalState externalState)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    BillDemand billDemand = GetBillDemand(context, billDemandUid);
                    billDemand.DateOfPerformance = externalState.PaymentDate;
                    billDemand.AccountNumber = externalState.DocumentNumber;
                    context.SubmitChanges();
                    context.spRecalcBillDemandCurrencySum(billDemandUid);
                }
                scope.Complete();
            }
        }

        public PaymentKind GetBillDemandPaymentKind(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var billDemand = GetBillDemand(context, billDemandUid);
                    byte paymentKindId = (byte)(billDemand.PaymentKindId.HasValue ? billDemand.PaymentKindId.Value : 0);
                    return PaymentKind.All.Single(p => p.Id == paymentKindId);
                }
            }
        }

        private Guid? GetInitiatorId(Guid billDemandUid, Budget2DataContext context)
        {
            return context.BillDemands.First(p => p.Id == billDemandUid).AuthorId;
        }



         private void WritePreHistory(Guid billDemandUid, Budget2DataContext context, WorkflowState initialState,
                                     WorkflowState destinationState, Guid? expectedInitiatorId, WorkflowState startState)
         {
             WritePreHistory(billDemandUid, context, initialState, destinationState, expectedInitiatorId, WorkflowCommand.Sighting, startState);
             
         }

        private void WritePreHistory(Guid billDemandUid, Budget2DataContext context, WorkflowState initialState,
                                     WorkflowState destinationState, Guid? expectedInitiatorId, WorkflowCommand command, WorkflowState startState)
        {
            if (initialState.Order < startState.Order)
                return;

            var billDemndHistoryItem = new BillDemandTransitionHistory
                                           {
                                               Id = Guid.NewGuid(),
                                               BillDemandId = billDemandUid,
                                               DestinationStateId = destinationState.DbStateId.Value,
                                               InitialStateId = initialState.DbStateId.Value,
                                               TransitionExpectedInitiatorId = expectedInitiatorId,
                                               CommandId = command.Id,
                                               IsPlanTransition = true,
                                               Description = string.Empty
                                           };
            context.BillDemandTransitionHistories.InsertOnSubmit(billDemndHistoryItem);
        }


        public void UpdateBillDemandState(WorkflowState state, Guid billDemandUid)
        {
            if (!state.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию BillDemand", "state");

            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var billDemand = context.BillDemands.SingleOrDefault(p => p.Id == billDemandUid);
                    if (billDemand == null)
                        return;
                    var billDemandState = context.BillDemandStates.SingleOrDefault(p => p.Id == state.DbStateId);
                    if (billDemandState == null)
                        return;
                    billDemand.BillDemandStateId = state.DbStateId.Value;
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public void UpdateBillDemandState(WorkflowState initialState, WorkflowState destinationState,
                                          WorkflowCommand command, Guid billDemandUid,
                                          Guid initiatorId, Guid? impesonatedIdentityId, string comment)
        {
            if (!initialState.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию BillDemand", "initialState");
            if (!destinationState.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию BillDemand",
                    "destinationState");
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    BillDemandTransitionHistory billDemndHistoryItem = null;

                    var billDemndHistoryItems = context.BillDemandTransitionHistories.Where(
                        p =>
                        p.BillDemandId == billDemandUid && p.InitialStateId == initialState.DbStateId.Value
                        && (p.CommandId == command.Id || command.SkipCheckCommandId) && !p.TransitionInitiatorId.HasValue).ToList();

                    if (billDemndHistoryItems.Count == 1)
                        billDemndHistoryItem = billDemndHistoryItems.First();
                    else if (billDemndHistoryItems.Count > 1)
                    {
                        billDemndHistoryItem = GetBillDemndHistoryItem(billDemndHistoryItems, impesonatedIdentityId,
                                                                       initiatorId);
                    }

                    if (billDemndHistoryItem == null)
                    {
                        billDemndHistoryItem = new BillDemandTransitionHistory
                                                   {
                                                       Id = Guid.NewGuid(),
                                                       BillDemandId = billDemandUid,
                                                       DestinationStateId = destinationState.DbStateId.Value,
                                                       InitialStateId = initialState.DbStateId.Value,
                                                       CommandId =
                                                           (command.Id == WorkflowCommand.Unknown.Id
                                                                ? (Guid?)null
                                                                : command.Id)
                                                   };
                        context.BillDemandTransitionHistories.InsertOnSubmit(billDemndHistoryItem);
                    }
                    billDemndHistoryItem.DestinationStateId = destinationState.DbStateId.Value;
                    billDemndHistoryItem.TransitionInitiatorId = initiatorId;
                    billDemndHistoryItem.TransitionTime = DateTime.Now;
                    billDemndHistoryItem.Comment = comment;
                    var info = WorkflowStateService.GetWorkflowStateInfo(destinationState);
                    billDemndHistoryItem.Description = WorkflowCommand.GetCommandDescription(command,
                                                                                             info == null
                                                                                                 ? string.Empty
                                                                                                 : info.StateVisibleName);
                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        private BillDemandTransitionHistory GetBillDemndHistoryItem(
            List<BillDemandTransitionHistory> billDemndHistoryItems, Guid? impesonatedIdentityId, Guid initiatorId)
        {
            BillDemandTransitionHistory billDemndHistoryItem;
            if (impesonatedIdentityId.HasValue)
                billDemndHistoryItem =
                    billDemndHistoryItems.FirstOrDefault(
                        p => p.TransitionExpectedInitiatorId == impesonatedIdentityId.Value);
            else
                billDemndHistoryItem =
                    billDemndHistoryItems.FirstOrDefault(
                        p => p.TransitionExpectedInitiatorId == initiatorId);
            return billDemndHistoryItem;
        }

        private bool LimitSight(Guid billDemandUid, Guid sighterId, Guid initiatorId, SightingType sightingType,
                                Func<Budget2DataContext, Guid, List<LimitSighter>> sighterSelector)
        {
            bool retval = false;
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var sighters = sighterSelector(context, billDemandUid);

                    if (sighters.Count(p => p.SighterId == sighterId) == 0)
                        return false;

                    List<LimitSighter> currentSighters = GetCurrentSighters(context, billDemandUid, sightingType);

                    foreach (var limitSighter in sighters.Where(p => p.SighterId == sighterId))
                    {
                        var sighter = limitSighter;
                        if (currentSighters.Count(p => p.LimitId == sighter.LimitId && p.SighterId == sighter.SighterId) == 0)
                        {
                            var newSighting = new WorkflowSighting
                            {
                                EntityId = billDemandUid,
                                Id = Guid.NewGuid(),
                                SighterId = sighterId,
                                InitiatorId = initiatorId,
                                SightingType = sightingType.Id,
                                SightingTime = DateTime.Now,
                                ItemId = sighter.LimitId
                            };
                            context.WorkflowSightings.InsertOnSubmit(newSighting);
                            currentSighters.Add(sighter);
                        }
                    }

                    retval =
                        sighters.TrueForAll(
                            p =>
                            currentSighters.FirstOrDefault(s => s.LimitId == p.LimitId && s.SighterId == p.SighterId) !=
                            null);
                    context.SubmitChanges();
                }

                scope.Complete();
            }
            return retval;
        }

        private List<LimitSighter> GetCurrentSighters(Budget2DataContext context, Guid billDemandUid, SightingType sightingType)
        {
            return context.WorkflowSightings.Where(
                p =>
                p.EntityId == billDemandUid &&
                p.SightingType == sightingType.Id).Select(
                    p => new LimitSighter { LimitId = p.ItemId, SighterId = p.SighterId, InitiatorId = p.InitiatorId})
                .Distinct().ToList();
        }

        private List<LimitSighter> GetLimitExecutorSighters(Budget2DataContext context, Guid billDemandUid)
        {
            return context.BillDemands.Single(p => p.Id == billDemandUid).BillDemandDistributions.Where(
              p =>
              p.DemandId.HasValue && p.Demand.LimitId.HasValue &&
              p.Demand.Limit.ExecutorId.HasValue).Select(
                  p => new LimitSighter { LimitId = p.Demand.LimitId.Value, SighterId = p.Demand.Limit.ExecutorId.Value }).Distinct().ToList();
        }


        private List<LimitSighter> GetLimitManagerSighters(Budget2DataContext context, Guid billDemandUid)
        {
            return context.BillDemands.Single(p => p.Id == billDemandUid).BillDemandDistributions.Where(
                p =>
                p.DemandId.HasValue && p.Demand.LimitId.HasValue &&
                p.Demand.Limit.ManagerId.HasValue).Select(
                    p =>
                    new LimitSighter { LimitId = p.Demand.LimitId.Value, SighterId = p.Demand.Limit.ManagerId.Value }).
                Distinct().ToList();
        }

        public List<LimitSighter> GetLimitManagerSightings(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    return GetCurrentSighters(context, billDemandUid, SightingType.BillDemandLimitManagerSighting);
                }
            }
        }

        public bool CheckPaymentPlanFilled(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    return
                        context.BillDemandContractMoneys.Count(
                            bdcm => bdcm.BillDemandId == billDemandUid && bdcm.ToPayment) > 0;
                }
            }
        }

        public List<LimitSighter> GetLimitManagerSighters(Guid billDemandUid)
        {
             using (var scope = ReadCommittedSupressedScope)
             {
                 using (var context = this.CreateContext())
                 {
                    return GetLimitManagerSighters(context, billDemandUid);
                 }
             }
        }


        public bool LimitExecutorSight(Guid billDemandUid, Guid sighterId, Guid initiatorId)
        {
            return LimitSight(billDemandUid, sighterId, initiatorId, SightingType.BillDemandLimitExecutorSighting,
                              GetLimitExecutorSighters);
        }

        private List<Guid> GetLimitsExecutors(Budget2DataContext context, Guid billDemandUid)
        {
            return context.BillDemands.Single(p => p.Id == billDemandUid).BillDemandDistributions.Where(
                p =>
                p.DemandId.HasValue && p.Demand.LimitId.HasValue &&
                p.Demand.Limit.ExecutorId.HasValue).Select(
                    p => p.Demand.Limit.ExecutorId.Value).Distinct().ToList();
        }

        public bool LimitManagerSight(Guid billDemandUid, Guid sighterId, Guid initiatorId)
        {
            return LimitSight(billDemandUid, sighterId, initiatorId, SightingType.BillDemandLimitManagerSighting,
                              GetLimitManagerSighters);
        }

        private List<Guid> GetLimitManagers(Budget2DataContext context, Guid billDemandUid)
        {
            return context.BillDemands.Single(p => p.Id == billDemandUid).BillDemandDistributions.Where(
                p =>
                p.DemandId.HasValue && p.Demand.LimitId.HasValue &&
                p.Demand.Limit.ManagerId.HasValue).Select(
                    p => p.Demand.Limit.ManagerId.Value).Distinct().ToList();
        }

        public void LimitExecutorResetSights(Guid billDemandUid)
        {
            DeleteSights(billDemandUid, SightingType.BillDemandLimitExecutorSighting);
        }

        private void DeleteSights(Guid billDemandUid, SightingType type)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var sights =
                        context.WorkflowSightings.Where(
                            p =>
                            p.EntityId == billDemandUid &&
                            p.SightingType == type.Id);

                    context.WorkflowSightings.DeleteAllOnSubmit(sights);

                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        public void LimitManagerResetSights(Guid billDemandUid)
        {
            DeleteSights(billDemandUid, SightingType.BillDemandLimitManagerSighting);
        }

        public decimal GetBillDemandValue(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var billDemand = context.BillDemands.Single(p => p.Id == billDemandUid);
                    return billDemand.Sum;
                }
            }
        }

        public bool CheckInitiatorHeadMustSign(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var executorStructDivisionCodes =
                        context.BillDemandDistributions.Where(p => p.BillDemandId == billDemandUid).Select(
                            p => p.Demand.ExecutorStructId).Distinct();

                    var initiatorDivisionId =
                        context.BillDemands.Where(p => p.Id == billDemandUid).Select(
                            p => p.Author.Employees.First().StructDivisionId).First();
                    return executorStructDivisionCodes.Count(p => p == initiatorDivisionId) <= 0;
                }
            }
        }


        public bool CheckUPKZHeadMustSight(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    return CheckUPKZHeadMustSight(GetBillDemand(context, billDemandUid));
                }
            }
        }

        private bool CheckUPKZHeadMustSight(BillDemand billDemand)
        {
            return billDemand.Sum >= Common.Settings.Instance.BillDemandWfLimitSum;
        }

        public bool CheckInitiatorIsHead(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var billDemand = GetBillDemand(context, billDemandUid);
                    return (billDemand.AuthorId.HasValue &&
                            (SecurityEntityService.CheckTrusteeWithIdIsInRole(billDemand.AuthorId.Value,
                                                                              BudgetRole.DivisionHead)));
                }
            }
        }

        public Guid GetBillDemandInitiatorId(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    return context.BillDemands.Where(p => p.Id == billDemandUid && p.AuthorId.HasValue).Select(p => p.AuthorId.Value).First();
                }
            }
        }

        public BillDemand GetBillDemand (Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var dlo = new DataLoadOptions();
                    dlo.LoadWith<BillDemand>(p=>p.Author);
                    context.LoadOptions = dlo;
                    return GetBillDemand(context, billDemandUid);
                }
            }
        }

        //private bool IsBillDemandFilialSupportExport(BillDemand billDemand)
        //{
        //    return true;
        //    //return billDemand.Filial.IsBillDemandExportable && billDemand.Contract != null && billDemand.Contract.CustomerCounteragent != null
        //    //            && billDemand.Contract.CustomerCounteragent.Number.ToString(CultureInfo.InvariantCulture) == BossAvailiableExecutorCode;
        //}

        public bool IsBillDemandFilialSupportExport(Guid billDemandUid)
        {
            return true;
            //using (var scope = ReadCommittedSupressedScope)
            //{
            //    using (var context = this.CreateContext())
            //    {
            //        var dlo = new DataLoadOptions();
            //        dlo.LoadWith<BillDemand>(p => p.Filial);
            //        dlo.LoadWith<BillDemand>(p => p.Contract);
            //        context.LoadOptions = dlo;
            //        var billDemand = GetBillDemand(context, billDemandUid);
            //        return IsBillDemandFilialSupportExport(billDemand);
            //    }
            //}
        }

        private string BossAvailiableExecutorCode
        {
            get { return ConfigurationManager.AppSettings.Get("BossAvailiableExecutorCode"); }
        }

        public BillDemandForExport GetBillDemandForExport(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var dlo = new DataLoadOptions();
                    dlo.LoadWith<BillDemand>(p => p.Contract);
                    dlo.LoadWith<BillDemand>(p => p.Currency);
                    dlo.LoadWith<BillDemand>(p => p.Currency1);
                    dlo.LoadWith<BillDemand>(p => p.Filial);
                    dlo.LoadWith<BillDemand>(p => p.Counteragent);
                    dlo.LoadWith<BillDemand>(p=>p.BillDemandContractMoneys);
                    dlo.LoadWith<Currency>(p => p.CurrencyRateActual);
                    dlo.LoadWith<Contract>(p => p.ContractMoneys);
                    dlo.LoadWith<Contract>(p => p.CustomerCounteragent);
                    //context.DeferredLoadingEnabled = false;
                    context.LoadOptions = dlo;

                    BillDemand billDemand = GetBillDemand(context, billDemandUid);

                    if (billDemand.ContractId == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить расходный документ не привязанный к контракту BillDemandId = {0}",
                                billDemand.Id));
                    if (billDemand.CurrencyId == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить расходный документ для котрого не задана валюта BillDemandId = {0}",
                                billDemand.Id));
                    if (billDemand.PaymentCurrencyId == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить расходный документ для котрого не задана платежная валюта BillDemandId = {0}",
                                billDemand.Id));

                    if (billDemand.CounteragentId == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить расходный документ для котрого не задан Контрагент BillDemandId = {0}",
                                billDemand.Id));

                    if (!billDemand.Currency.IsBase && billDemand.Currency.CurrencyRateActual == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить расходный документ не найден актуальный курс BillDemandId = {0}",
                                billDemand.Id));
                    if (billDemand.AllocationDate == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить расходный документ для котрого не задана дата отправки на маршрут BillDemandId = {0}",
                                billDemand.Id));

                    if (billDemand.FilialId == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить расходный документ для котрого не задан Плательщик BillDemandId = {0}",
                                billDemand.Id));

                    if (billDemand.Contract.CustomerCounteragent == null)
                        throw new InvalidOperationException(
                                string.Format(
                                    "Невозможно выгрузить расходный документ для конракта которого не задан контрагент заказчик BillDemandId = {0}",
                                    billDemand.Id));

                    if (billDemand.Contract.ExternalId == null)
                        throw new InvalidOperationException(
                            string.Format(
                                "Невозможно выгрузить контракт не загруженный из БОСС  BillDemandId = {0} (ExternalId = {1}) ",
                                billDemand.Id, billDemand.Contract.ExternalId));
                    
                    var billDemandForExport = new BillDemandForExport()
                                                  {
                                                      Id = billDemand.Id,
                                                      ContractId = billDemand.Contract.ExternalId.Value,
                                                      CurrencyCode = billDemand.Currency1.IsBase ? billDemand.Currency.Code : billDemand.Currency1.Code,
                                                      CurrencyRevisionDate =
                                                          ((billDemand.CurrencyRateTypeId.HasValue &&
                                                           billDemand.CurrencyRateTypeId >= 2) || billDemand.Currency.IsBase)
                                                              ? billDemand.AllocationDate.Value
                                                              : billDemand.Currency.CurrencyRateActual.ReevaluationDate,
                                                      CustomerCounteragentId = billDemand.Contract.CustomerCounteragent.Number,
                                                      CounteragentId = billDemand.Counteragent.Number,
                                                      Number = billDemand.Number,
                                                      IdNumber = billDemand.IdNumber,
                                                      SumWIthNDS = billDemand.Sum,
                                                      NDSTaxValue = billDemand.Currency1.IsBase ? (billDemand.NDSValue.HasValue ? billDemand.NDSValue.Value : (billDemand.Sum - (billDemand.SumWithoutNDS.HasValue ? billDemand.SumWithoutNDS.Value : 0))) : (billDemand.CurrencySum.HasValue ? billDemand.CurrencySum.Value : 0),
                                                      DocumentDate = billDemand.AllocationDate.Value,
                                                      AccountDate = billDemand.AccountDate,
                                                      FirmCode = billDemand.Filial.Code
                                                  };

                    billDemandForExport.PaymentPlanItemIds =
                        billDemand.Contract.ContractMoneys.Where(p => p.ExternalId.HasValue && billDemand.BillDemandContractMoneys.Count(bdcm => bdcm.Month == p.MonthId && bdcm.Year == p.Year && bdcm.ToPayment) > 0).Select(
                            p => p.ExternalId.Value).Distinct().ToList();


                    var firstDistribution = billDemand.BillDemandDistributions.FirstOrDefault();
                    if (firstDistribution!= null && firstDistribution.Demand != null)
                    {
                        if (firstDistribution.Demand.Project != null)
                            billDemandForExport.ProjectCode = firstDistribution.Demand.Project.Code;
                        if (firstDistribution.Demand.ExecutorStructDivision != null)
                            billDemandForExport.OPCode = firstDistribution.Demand.ExecutorStructDivision.Code;
                    }

                    var firstAllocation = billDemand.BillDemandAllocations.FirstOrDefault();
                    if (firstAllocation != null)
                    {
                        if (firstAllocation.CostArticle != null)
                            billDemandForExport.SmetaCode = firstAllocation.CostArticle.Code;
                        if (firstAllocation.CFO != null)
                            billDemandForExport.PPCode = firstAllocation.CFO.Code;
                    }


                    return billDemandForExport;
                }
            }
        }

        private BillDemand GetBillDemand(Budget2DataContext context, Guid billDemandUid)
        {
            return context.BillDemands.First(p => p.Id == billDemandUid);
        }

        public void DeleteDemandPermissions(Guid billDemandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var permissions = context.DemandPermissions.Where(
                        p =>
                        p.Demand.BillDemandDistributions.Count(
                            bdd => bdd.BillDemand.AuthorId == p.UserId && bdd.BillDemandId == billDemandUid) > 0
                        && !p.IsRestricted && !p.IsConstantPermission);
                    foreach (var demandPermission in permissions)
                    {
                        demandPermission.IsRestricted = true;
                        demandPermission.EndPermissionDate = DateTime.Now;
                    }
                    context.SubmitChanges();
                   // context.BillDemands.Where(p=>p.Id = billDemandUid && p.BillDemandDistributions.Where(d=>d.) )
                }
                scope.Complete();
            }
        }
    }
}