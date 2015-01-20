using System;
using System.Linq;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Microsoft.Practices.CompositeWeb;

namespace Budget2.Server.Business.Services
{
    public class DemandBusinessService : Budget2DataContextService, IDemandBusinessService
    {
        [ServiceDependency]
        public IWorkflowStateService WorkflowStateService { get; set; }

        public void CreateDemandPreHistory(Guid demandUid, WorkflowState state)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var existingNotUsedItems =
                      context.DemandTransitionHistories.Where(
                          dth => dth.DemandId == demandUid && !dth.TransitionTime.HasValue).ToList();

                    context.DemandTransitionHistories.DeleteAllOnSubmit(existingNotUsedItems);

                    var demand = GetDemand(context, demandUid);
                    if (CheckInitiatorIsExecutorStructDivision(demandUid, context))
                    {
                        WritePreHistory(demandUid, context, WorkflowState.DemandDraft, WorkflowState.DemandOPHeadSighting, demand.AuthorId, state);
                    }
                    else
                    {
                        WritePreHistory(demandUid, context, WorkflowState.DemandDraft, WorkflowState.DemandOPExpertSighting, demand.AuthorId, state);
                        WritePreHistory(demandUid, context, WorkflowState.DemandOPExpertSighting, WorkflowState.DemandInitiatorHeadSighting, null, state);
                        WritePreHistory(demandUid, context, WorkflowState.DemandInitiatorHeadSighting, WorkflowState.DemandOPHeadSighting, null, state);
                    }
                    WritePreHistory(demandUid, context, WorkflowState.DemandOPHeadSighting, WorkflowState.DemandUPKZCuratorSighting, null, state);
                    WritePreHistory(demandUid, context, WorkflowState.DemandUPKZCuratorSighting, WorkflowState.DemandUPKZHeadSighting, null, state);
                    WritePreHistory(demandUid, context, WorkflowState.DemandUPKZHeadSighting, WorkflowState.DemandAgreed, null, state);

                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        private void WritePreHistory(Guid demandId, Budget2DataContext context, WorkflowState initialState,
                                    WorkflowState destinationState, Guid? expectedInitiatorId, WorkflowState startState)
        {

            if (initialState.Order < startState.Order)
                return;
            var billDemndHistoryItem = new DemandTransitionHistory
            {
                Id = Guid.NewGuid(),
                DemandId = demandId,
                DestinationStateId = destinationState.DbStateId.Value,
                InitialStateId = initialState.DbStateId.Value,
                TransitionExpectedInitiatorId = expectedInitiatorId,
                CommandId = WorkflowCommand.Sighting.Id,
                Comment = string.Empty,
                Description = string.Empty
            };
            context.DemandTransitionHistories.InsertOnSubmit(billDemndHistoryItem);
        }

        public bool CheckInitiatorIsExecutorStructDivision(Guid demandUid)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    return CheckInitiatorIsExecutorStructDivision(demandUid, context);
                }
            }
        }

        private bool CheckInitiatorIsExecutorStructDivision(Guid demandUid, Budget2DataContext context)
        {
            return
                        context.Demands.Count(
                            p =>
                            p.Id == demandUid && p.AuthorId.HasValue &&
                            p.Author.Employees.First().StructDivisionId == p.ExecutorStructId) > 0;
        }

        public void UpdateDemandState(WorkflowState state, Guid demandId)
        {
            if (!state.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию Demand", "state");

            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    Demand demand = GetDemand(context, demandId);
                    if (demand == null)
                        return;
                    var demandState = context.DemandStatusInternals.SingleOrDefault(p => p.Id == state.DbStateId);
                    if (demandState == null)
                        return;
                    demand.InternalStatusId = state.DbStateId.Value;
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public Demand GetDemand (Guid demandId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    return GetDemand(context, demandId);
                }
            }
        }

        private Demand GetDemand(Budget2DataContext context, Guid demandId)
        {
            var demand = context.Demands.SingleOrDefault(p => p.Id == demandId);
            return demand;
        }

        public void UpdateDemandState(WorkflowState initialState, WorkflowState destinationState, WorkflowCommand command, Guid demandId,
                                         Guid initiatorId, string comment)
        {
            if (!initialState.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию Demand", "initialState");
            if (!destinationState.DbStateId.HasValue)
                throw new ArgumentException(
                    "Не определено соттветствие состояния Workflow отображаемому состоянию Demand",
                    "destinationState");
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var demandHistoryItem =
                     context.DemandTransitionHistories.Where(
                         p =>
                         p.DemandId == demandId && p.InitialStateId == initialState.DbStateId.Value && p.DestinationStateId == destinationState.DbStateId.Value
                         && (p.CommandId == command.Id || command.SkipCheckCommandId) && !p.TransitionInitiatorId.HasValue).ToList().FirstOrDefault();

                    if (demandHistoryItem == null)
                    {
                        demandHistoryItem = new DemandTransitionHistory()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DemandId = demandId,
                                                    DestinationStateId = destinationState.DbStateId.Value,
                                                    InitialStateId = initialState.DbStateId.Value,
                                                    CommandId =
                                                        (command.Id == WorkflowCommand.Unknown.Id
                                                             ? (Guid?) null
                                                             : command.Id),
                                                };
                        context.DemandTransitionHistories.InsertOnSubmit(demandHistoryItem);
                    }
                    demandHistoryItem.TransitionInitiatorId = initiatorId;
                    demandHistoryItem.TransitionTime = DateTime.Now;
                    demandHistoryItem.Comment = comment;
                    var info = WorkflowStateService.GetWorkflowStateInfo(destinationState);
                    demandHistoryItem.Description = WorkflowCommand.GetCommandDescription(command,
                                                                                             info == null
                                                                                                 ? string.Empty
                                                                                                 : info.StateVisibleName);



                    
                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

    }
}
