using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Common.Utils;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Server.Business.Services
{
    public class WorkflowStateService : Budget2DataContextService, IWorkflowStateService
    {
        public class WorkflowDbState
        {
            public string Name { get; set; }
            public byte Id { get; set; }
        }

        private DictionaryCache<byte, WorkflowDbState> _billDemandStateCache;

        private DictionaryCache<byte, WorkflowDbState> _demandStateCache;

        private DictionaryCache<byte, WorkflowDbState> _demandAdjustmentStateCache;

        private DictionaryCache<WorkflowState, WorkflowStateInfo> _workflowStateInfosCache;

        public WorkflowStateInfo GetWorkflowStateInfo (WorkflowState state)
        {
            return _workflowStateInfosCache.GetValues().FirstOrDefault(v => v.StateSystemName == state.WorkflowStateName);
        }

        public WorkflowStateService ()
        {
            _billDemandStateCache = new DictionaryCache<byte, WorkflowDbState>(new TimeSpan(0,30,0),FillBillDemandStates);
            _demandStateCache = new DictionaryCache<byte, WorkflowDbState>(new TimeSpan(0, 30, 0), FillDemandStates);
            _demandAdjustmentStateCache = new DictionaryCache<byte, WorkflowDbState>(new TimeSpan(0, 30, 0), FillDemandAdjustmentStates);
            _workflowStateInfosCache = new DictionaryCache<WorkflowState, WorkflowStateInfo>(new TimeSpan(0, 30, 0), FillWorkflowStateInfos);

        }

        private  Dictionary<WorkflowState, WorkflowStateInfo> FillWorkflowStateInfos ()
        {
            var infos = new Dictionary<WorkflowState, WorkflowStateInfo>();

            foreach (var workflowState in WorkflowState.AllStates.Where(s=>!s.ExcludeFromSetStateCommand))
            {
                WorkflowDbState dbState = null;
                if (workflowState.DbStateId.HasValue)
                {
                    if (workflowState.Type == WorkflowType.BillDemandWorkfow)
                        dbState = _billDemandStateCache.GetValue(workflowState.DbStateId.Value);
                    else if (workflowState.Type == WorkflowType.DemandWorkflow)
                        dbState = _demandStateCache.GetValue(workflowState.DbStateId.Value);
                    else if (workflowState.Type == WorkflowType.DemandAdjustmentWorkflow)
                        dbState = _demandAdjustmentStateCache.GetValue(workflowState.DbStateId.Value);
                }

                if (dbState != null)
                {
                    infos.Add(workflowState, new WorkflowStateInfo()
                                                 {
                                                     StateSystemName = workflowState.WorkflowStateName,
                                                     StateVisibleName =
                                                         string.IsNullOrEmpty(workflowState.StateNamePostfix)
                                                             ? dbState.Name
                                                             : string.Format("{0} {1}", dbState.Name,
                                                                             workflowState.StateNamePostfix),
                                                    WorkflowTypeId = workflowState.Type.Id
                                                 }
                        );

                }
            }

            return infos;
        }

        private Dictionary<byte, WorkflowDbState> FillBillDemandStates()
        {
            var result = new Dictionary<byte, WorkflowDbState>();
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    var billDemandStates = context.BillDemandStates.Select(p => p);

                    foreach (var billDemandState in billDemandStates)
                    {
                        result.Add(billDemandState.Id,
                                   new WorkflowDbState() {Id = billDemandState.Id, Name = billDemandState.Name});
                    }
                }
            }

            return result;
        }

        private Dictionary<byte, WorkflowDbState> FillDemandStates()
        {
            var result = new Dictionary<byte, WorkflowDbState>();
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    var demandStates = context.DemandStatusInternals.Select(p => p);

                    foreach (var demandState in demandStates)
                    {
                        result.Add(demandState.Id,
                                   new WorkflowDbState() { Id = demandState.Id, Name = demandState.Name });
                    }
                }
            }

            return result;
        }

        private Dictionary<byte, WorkflowDbState> FillDemandAdjustmentStates()
        {
            var result = new Dictionary<byte, WorkflowDbState>();
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = this.CreateContext())
                {
                    var demandAdjustmentStates = context.DemandAdjustmentStates.Select(p => p);

                    foreach (var demandAdjustmentState in demandAdjustmentStates)
                    {
                        result.Add(demandAdjustmentState.Id,
                                   new WorkflowDbState() { Id = demandAdjustmentState.Id, Name = demandAdjustmentState.Name });
                    }
                }
            }

            return result;
        }

        public WorkflowState GetCurrentState(Guid instanceId)
        {
            using (var context = this.CreateContext())
            {
                var currentState = context.WorkflowCurrentStates.SingleOrDefault(p => p.WorkflowId == instanceId); 
                if (currentState == null)
                    return null; 

                var currentTypedState =
                    WorkflowState.AllStates.FirstOrDefault(
                        p => p.Type.Id == currentState.WorkflowTypeId && p.WorkflowStateName == currentState.StateName);

                if (currentTypedState == null)
                    throw new ArgumentException("Unknown WF type"); //TODO Typed faults

                return currentTypedState;
            }
        }

        /// <summary>
        /// Пытается определить тип воркфлоу по Id сущности,
        /// Если определение невозможно возвращает null
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public WorkflowType TryGetExpectedWorkflowType(Guid instanceId)
        {
            using (var context = this.CreateContext())
            {
                if (context.BillDemands.Count(p => p.Id == instanceId) == 1)
                    return WorkflowType.BillDemandWorkfow;
                else if (context.DemandAdjustments.Count(p => p.Id == instanceId) == 1)
                    return WorkflowType.DemandAdjustmentWorkflow;
                else if (context.Demands.Count(p=>p.Id == instanceId) == 1)
                    return WorkflowType.DemandWorkflow;
                else
                    return null;
            }
        }

        public WorkflowState GetWorkflowState (Guid instanceId)
        {
            var state = GetCurrentState(instanceId);

            if (state != null)
                return state;

            var workflowType = TryGetExpectedWorkflowType(instanceId);
            if (workflowType == null)
                throw new InvalidOperationException("Невозможно вернуть состояние и определить тип воркфлоу");
            return WorkflowState.AllStates.First(ws => ws.Type.Id == workflowType.Id && ws.IsInitial); 
        }

        public List<WorkflowStateInfo> GetAllAvailiableStates(Guid instanceId)
        {
            var state = GetCurrentState(instanceId);

            if (state != null)
                return
                    _workflowStateInfosCache.GetDictionary().Where(p => p.Key.Type == state.Type).Select(p => p.Value).
                        ToList();

            var workflowType = TryGetExpectedWorkflowType(instanceId);
            if (workflowType == null)
                return new List<WorkflowStateInfo>();

            return _workflowStateInfosCache.GetDictionary().Where(p => p.Key.Type == workflowType).Select(p => p.Value).
                ToList();
        }


    }
}
