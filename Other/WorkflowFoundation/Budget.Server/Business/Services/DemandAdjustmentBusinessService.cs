using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Microsoft.Practices.CompositeWeb;

namespace Budget2.Server.Business.Services
{
    public class DemandAdjustmentBusinessService : Budget2DataContextService, IDemandAdjustmentBusinessService
    {
        [ServiceDependency]
        public ISecurityEntityService SecurityEntityService { get; set; }
        [ServiceDependency]
        public IWorkflowStateService WorkflowStateService { get; set; }


        public void CreateDemandAdjustmentPreHistory(Guid demandAdjustmentId, WorkflowState state)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                { 
                    var demand = GetDemandAdjustmentWithAllLimits(context, demandAdjustmentId);
                    
                    var existingNotUsedItems =
                    context.DemandAdjustmentTransitionHistories.Where(
                        dath => dath.DemandAdjustmentId == demandAdjustmentId && !dath.TransitionTime.HasValue).ToList();

                    context.DemandAdjustmentTransitionHistories.DeleteAllOnSubmit(existingNotUsedItems);

                   

                    if (demand.Kind != DemandAdjustmentType.Redistribution.Id)
                        return;

                    if (!CheckLimitIsContingency(demand))
                    {

                        WritePreHistory(demandAdjustmentId, context, WorkflowState.DemandAdjustmentDraft,
                                        WorkflowState.DemandAdjustmentSourceDemandLimitExecutorSighting,
                                        demand.CreatorId, state);

                        WritePreHistory(demandAdjustmentId, context,
                                        WorkflowState.DemandAdjustmentSourceDemandLimitExecutorSighting,
                                        WorkflowState.DemandAdjustmentSourceDemandLimitManagerSighting,
                                        demand.SourceDemand != null && demand.SourceDemand.Limit != null
                                            ? demand.SourceDemand.Limit.ExecutorId
                                            : null, state);

                        WritePreHistory(demandAdjustmentId, context,
                                        WorkflowState.DemandAdjustmentSourceDemandLimitManagerSighting,
                                        WorkflowState.DemandAdjustmentTargetDemandLimitExecutorSighting,
                                        demand.SourceDemand != null && demand.SourceDemand.Limit != null
                                            ? demand.SourceDemand.Limit.ManagerId
                                            : null, state);
                            WritePreHistory(demandAdjustmentId, context,
                                            WorkflowState.DemandAdjustmentTargetDemandLimitExecutorSighting,
                                            WorkflowState.DemandAdjustmentTargetDemandLimitManagerSighting,
                                            demand.TargetDemand != null && demand.TargetDemand.Limit != null
                                                ? demand.TargetDemand.Limit.ExecutorId
                                                : null, state);

                            if (!CheckToSkipUPKZSighting(demand))
                            {
                                WritePreHistory(demandAdjustmentId, context,
                                                WorkflowState.DemandAdjustmentTargetDemandLimitManagerSighting,
                                                WorkflowState.DemandAdjustmentUPKZCuratorSighting,
                                                demand.TargetDemand != null && demand.TargetDemand.Limit != null
                                                    ? demand.TargetDemand.Limit.ManagerId
                                                    : null, state);

                                WritePreHistory(demandAdjustmentId, context,
                                                WorkflowState.DemandAdjustmentUPKZCuratorSighting,
                                                WorkflowState.DemandAdjustmentAgreed,
                                                null, state
                                    );
                            }
                            else
                            {
                                WritePreHistory(demandAdjustmentId, context,
                                                WorkflowState.DemandAdjustmentTargetDemandLimitManagerSighting,
                                                WorkflowState.DemandAdjustmentAgreed,
                                                demand.TargetDemand != null && demand.TargetDemand.Limit != null
                                                    ? demand.TargetDemand.Limit.ManagerId
                                                    : null, state);
                            }
                       // }
                    }
                    else
                    {
                        WritePreHistory(demandAdjustmentId, context, WorkflowState.DemandAdjustmentDraft,
                                        WorkflowState.DemandAdjustmentUPKZHeadSighting, demand.CreatorId, state);
                        WritePreHistory(demandAdjustmentId, context, WorkflowState.DemandAdjustmentUPKZHeadSighting,
                                        WorkflowState.DemandAdjustmentAgreed, null, state);
                    }



                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }


        private void WritePreHistory(Guid demandAdjustmentId, Budget2DataContext context, WorkflowState initialState,
                                   WorkflowState destinationState, Guid? expectedInitiatorId, WorkflowState startState)
        {
            if (initialState.Order < startState.Order)
                return;
            var billDemndHistoryItem = new DemandAdjustmentTransitionHistory
            {
                Id = Guid.NewGuid(),
                DemandAdjustmentId = demandAdjustmentId,
                DestinationStateId = destinationState.DbStateId.Value,
                InitialStateId = initialState.DbStateId.Value,
                TransitionExpectedInitiatorId = expectedInitiatorId,
                CommandId = WorkflowCommand.Sighting.Id,
                Comment = string.Empty,
                Description = string.Empty
            };
            context.DemandAdjustmentTransitionHistories.InsertOnSubmit(billDemndHistoryItem);
        }




        public void UpdateDemandAdjustmentState(WorkflowState state, Guid demandAdjustmentId)
        {
            if (!state.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию DemandAdjustment", "state");

            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var demandAdjustment = context.DemandAdjustments.SingleOrDefault(p => p.Id == demandAdjustmentId);
                    if (demandAdjustment == null)
                        return;
                    var demandAdjustmentState = context.DemandAdjustmentStates.SingleOrDefault(p => p.Id == state.DbStateId);
                    if (demandAdjustmentState == null)
                        return;
                    demandAdjustment.State = state.DbStateId.Value;
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public DemandAdjustmentType GetDemandAdjustmentType(Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var demandAdjustment = context.DemandAdjustments.First(p => p.Id == demandAdjustmentId);
                    return DemandAdjustmentType.All.First(p => p.Id == demandAdjustment.Kind);
                }
            }
        }

        public void UpdateDemandAdjustmentState(WorkflowState initialState, WorkflowState destinationState, WorkflowCommand command, Guid demandAdjustmentUid,
                                         Guid initiatorId, string comment)
        {
            if (!initialState.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию DemandAdjustment", "initialState");
            if (!destinationState.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию DemandAdjustment",
                    "destinationState");
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var demandAdjustmentHistoryItem =
                   context.DemandAdjustmentTransitionHistories.Where(
                       p =>
                       p.DemandAdjustmentId == demandAdjustmentUid && p.InitialStateId == initialState.DbStateId.Value && p.DestinationStateId == destinationState.DbStateId.Value
                       && (p.CommandId == command.Id || command.SkipCheckCommandId) && !p.TransitionInitiatorId.HasValue).ToList().FirstOrDefault();

                    if (demandAdjustmentHistoryItem == null)
                    {
                        demandAdjustmentHistoryItem = new DemandAdjustmentTransitionHistory()
                        {
                            Id = Guid.NewGuid(),
                            DemandAdjustmentId = demandAdjustmentUid,
                            DestinationStateId = destinationState.DbStateId.Value,
                            InitialStateId = initialState.DbStateId.Value,
                            CommandId =
                                (command.Id == WorkflowCommand.Unknown.Id
                                     ? (Guid?)null
                                     : command.Id),
                        };
                        context.DemandAdjustmentTransitionHistories.InsertOnSubmit(demandAdjustmentHistoryItem);
                    }

                    demandAdjustmentHistoryItem.TransitionInitiatorId = initiatorId;
                    demandAdjustmentHistoryItem.TransitionTime = DateTime.Now;
                    demandAdjustmentHistoryItem.Comment = comment;
                    var info = WorkflowStateService.GetWorkflowStateInfo(destinationState);
                    demandAdjustmentHistoryItem.Description = WorkflowCommand.GetCommandDescription(command,
                                                                                             info == null
                                                                                                 ? string.Empty
                                                                                                 : info.StateVisibleName);




                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        private bool CheckLimitIsContingency (DemandAdjustment demandAdjustment)
        {
            return (demandAdjustment.SourceDemandId.HasValue && demandAdjustment.SourceDemand.Limit.TypeId == 1) ||
                   (demandAdjustment.TargetDemandId.HasValue && demandAdjustment.TargetDemand.Limit.TypeId == 1);
        }

        public bool CheckLimitIsContingency(Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var demandAdjustment = GetDemandAdjustmentWithAllLimits(context, demandAdjustmentId);
                    if (demandAdjustment == null)
                        return false;
                    return CheckLimitIsContingency(demandAdjustment);
                }
            }
        }

        public bool CheckLimitsManagersIsSame(Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    return
                        context.DemandAdjustments.Count(
                            p =>
                            p.Id == demandAdjustmentId && p.SourceDemandId.HasValue && p.TargetDemandId.HasValue &&
                            p.SourceDemand.Limit.ManagerId == p.TargetDemand.Limit.ManagerId) > 0;
                }
            }
        }

        private bool CheckToSkipUPKZSighting (DemandAdjustment da)
        {
            if (SecurityEntityService.CheckTrusteeWithIdIsInRole(da.CreatorId, BudgetRole.Curator)
                        || SecurityEntityService.CheckTrusteeWithIdIsInRole(da.CreatorId, BudgetRole.UPKZHead))
                return true;

            return false;
        }

    
        public bool CheckToSkipUPKZSighting (Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var demandAdjustment = GetDemandAdjustment(context, demandAdjustmentId);
                    return CheckToSkipUPKZSighting(demandAdjustment);

                }
            }
        }


        public bool CheckToSkipSourceDemandLimitManagerSighting(Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var da = GetDemandAdjustmentWithAllLimits(context, demandAdjustmentId);
                    try
                    {
                        CheckDemandAdjustmentLimits(da);
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }
                    
                    return da.SourceDemand.Limit.ExecutorId == da.SourceDemand.Limit.ManagerId;
                }
            }
        }

        
        private bool CheckToSkipTargetDemandLimitManagerSighting(DemandAdjustment da)
        {
            try
            {
                CheckDemandAdjustmentLimits(da);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            return (da.TargetDemand.Limit.ExecutorId == da.SourceDemand.Limit.ExecutorId && da.TargetDemand.Limit.ManagerId == da.SourceDemand.Limit.ManagerId) || (da.TargetDemand.Limit.ExecutorId == da.TargetDemand.Limit.ManagerId);
        }

        public bool CheckToSkipTargetDemandLimitManagerSighting(Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var da = GetDemandAdjustmentWithAllLimits(context, demandAdjustmentId);
                    return CheckToSkipTargetDemandLimitManagerSighting(da);
                }
            }
        }

        public DemandAdjustmentSighters GetSightersId(Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var da = GetDemandAdjustmentWithAllLimits(context, demandAdjustmentId);

                    var sourceDemandLimitExecutorLastRecord  =  da.DemandAdjustmentTransitionHistories.Where(
                        dath =>
                        dath.InitialStateId == WorkflowState.DemandAdjustmentSourceDemandLimitExecutorSighting.DbStateId)
                        .OrderByDescending(dath => dath.TransitionTime).Take(1).FirstOrDefault();

                    var sourceDemandLimitManagerLastRecord = da.DemandAdjustmentTransitionHistories.Where(
                        dath =>
                        dath.InitialStateId == WorkflowState.DemandAdjustmentSourceDemandLimitManagerSighting.DbStateId)
                        .OrderByDescending(dath => dath.TransitionTime).Take(1).FirstOrDefault();
                    

                    var sighters = new DemandAdjustmentSighters()
                                       {
                                           SourceDemandLimitExecutor =
                                               sourceDemandLimitExecutorLastRecord == null ? null : sourceDemandLimitExecutorLastRecord.TransitionInitiatorId,
                                           SourceDemandLimitManager =
                                               sourceDemandLimitManagerLastRecord == null ? null : sourceDemandLimitManagerLastRecord.TransitionInitiatorId,
                                        };
                    return sighters;
                }
            }
        }

        private bool CheckToSkipTargetDemandLimitExecutorSighting(DemandAdjustment demandAdjustment)
        {
            try
            {
                CheckDemandAdjustmentLimits(demandAdjustment);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return demandAdjustment.TargetDemand.Limit.ExecutorId == demandAdjustment.SourceDemand.Limit.ExecutorId && demandAdjustment.TargetDemand.Limit.ManagerId == demandAdjustment.SourceDemand.Limit.ManagerId;
        }

        public bool CheckToSkipTargetDemandLimitExecutorSighting(Guid demandAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var da = GetDemandAdjustmentWithAllLimits(context, demandAdjustmentId);
                    return CheckToSkipTargetDemandLimitExecutorSighting(da);
                }
            }
        }

        private void CheckDemandAdjustmentLimits (DemandAdjustment da)
        {
            if (da.TargetDemand == null || da.SourceDemand == null || da.TargetDemand.Limit == null || da.SourceDemand.Limit == null)
                throw new InvalidOperationException("В корректировке не заполнениы заявки и/или лимиты в этих заявках");
        }

        private DemandAdjustment GetDemandAdjustmentWithAllLimits(Budget2DataContext context, Guid demandAdjustmentUid)
        {
            DataLoadOptions dlo = new DataLoadOptions();
            dlo.LoadWith<DemandAdjustment>(p=>p.SourceDemand);
            dlo.LoadWith<DemandAdjustment>(p => p.TargetDemand);
            dlo.LoadWith<Demand>(p=>p.Limit);
            context.LoadOptions = dlo;
            return GetDemandAdjustment(context, demandAdjustmentUid);
        }
       

        private DemandAdjustment GetDemandAdjustment(Budget2DataContext context, Guid demandAdjustmentUid)
        {
            return context.DemandAdjustments.First(p => p.Id == demandAdjustmentUid);
        }

        public void SetStartProcessingDate (Guid demnadAdjustmentId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var da = GetDemandAdjustment(context, demnadAdjustmentId);

                    da.TransferDate = DateTime.Now;

                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }
    }
}