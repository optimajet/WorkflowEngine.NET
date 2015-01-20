using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Workflow.Interface.DataContracts;
using Budget2.Server.Workflow.Interface.Services;
using Common.WF;
using Microsoft.Practices.CompositeWeb;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;
using Demand = Budget2.Workflow.Demand;
using DemandAdjustment = Budget2.Workflow.DemandAdjustment;
using BillDemand = Budget2.Workflow.BillDemand;


namespace Budget2.Server.Workflow.Services
{
    public class WorkflowSupportService : Budget2DataContextService, IWorkflowSupportService
    {
        [ServiceDependency]
        public IBillDemandBuinessService BillDemandBuinessService { get; set; }

        [ServiceDependency]
        public IDemandAdjustmentBusinessService DemandAdjustmentBusinessService { get; set; }

        [ServiceDependency]
        public IDemandBusinessService DemandBusinessService { get; set; }

        [ServiceDependency]
        public IWorkflowStateService WorkflowStateService { get; set; }

        List<string> OldAssemblyNames = new List<string> { "Budget2.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Budget2.Workflow.1.0.0.0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=893bf0e5006fa3d1" };

        private Dictionary<Guid,object> _wfLocks = new Dictionary<Guid, object>();

        private volatile object _lock = new object();

        private object GetLockForWorkflow (Guid instanceId)
        {
            if (_wfLocks.ContainsKey(instanceId))
                return _wfLocks[instanceId];
            lock (_lock)
            {
                if (_wfLocks.ContainsKey(instanceId))
                    return _wfLocks[instanceId];
                else
                {
                    var wflock = new object();
                    _wfLocks.Add(instanceId,wflock);
                    return wflock;
                }
            }

        }


        public void UpgradeWorkflow(WorkflowRuntime runtime, Guid instanceId)
        {
            WorkflowInstance workflowInstance = runtime.GetWorkflow(instanceId);

            var definition = workflowInstance.GetWorkflowDefinition();
            if (!OldAssemblyNames.Contains(definition.GetType().Assembly.FullName))
                return;

            lock (GetLockForWorkflow(instanceId))
            {

                workflowInstance.Unload();

                var are = new AutoResetEvent(false);

                var parameters = new Dictionary<string, object>();

                //Получаем перзистанс и извлекаем состояние
                var persistance = runtime.GetService<NotTerminatingSqlWorkflowPersistenceService>();

                persistance.OnArgsAllowed +=
                    delegate(object sender, NotTerminatingSqlWorkflowPersistenceService.WorkflowSavedParametersArgs e)
                    {
                        if (e.InstanceId == instanceId)
                        {
                            parameters = e.Parameters;
                            are.Set();
                        }
                    };

                workflowInstance = runtime.GetWorkflow(instanceId);

                definition = workflowInstance.GetWorkflowDefinition();
                if (!OldAssemblyNames.Contains(definition.GetType().Assembly.FullName))
                    //Если версия изменилась то дальнейшие манипуляции не нужны
                    return;
                
                are.WaitOne(10000);

                workflowInstance.Unload();

                using (var context = this.CreateContext())
                {
                    context.DeleteWorkflowInPesistenceStore(instanceId);
                    context.SubmitChanges();
                }
                var workflowState = WorkflowStateService.GetWorkflowState(instanceId);

                parameters.Add(StateMachineWithSimpleContainer.DontWriteToWorkflowHistoryPersistenceKey, true);

                using (var sync = new WorkflowSync(runtime, instanceId))
                {
                    if (!CreateWorkflowIfNotExists(runtime, instanceId, workflowState.Type, parameters))
                        //Это ожидание создания воркфлоу
                    {
                        sync.WaitHandle.WaitOne(600000);
                    }

                }

                var wfinstance = new StateMachineWorkflowInstance(runtime, instanceId);
                using (var sync = new WorkflowSync(runtime, instanceId))
                    //Это ожидание завершения установки состояния воркфлоу
                {
                    wfinstance.SetState(workflowState.WorkflowStateName);
                    sync.WaitHandle.WaitOne(600000);
                }

                var args = new SetWorkflowInternalParametersEventArgs(instanceId,
                                                                      new Dictionary<string, object>()
                                                                          {
                                                                              {
                                                                                  StateMachineWithSimpleContainer.
                                                                                  DontWriteToWorkflowHistoryPersistenceKey
                                                                                  ,
                                                                                  false
                                                                                  }
                                                                          });
                SetInternalParameters(null, args);
            }

        }

        public bool TryUpgradeWorkflow(WorkflowRuntime runtime, Guid instanceId)
        {
            try
            {
                UpgradeWorkflow(runtime,instanceId);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public void RewriteWorkflow(Guid instanceId, WorkflowState state)
        {

            if (state.Type.Id == WorkflowType.BillDemandWorkfow.Id)
            {
                BillDemandBuinessService.CreateBillDemandPreHistory(instanceId, state);
            }
            else if (state.Type.Id == WorkflowType.DemandWorkflow.Id)
            {
                DemandBusinessService.CreateDemandPreHistory(instanceId, state);
            }
            else if (state.Type.Id == WorkflowType.DemandAdjustmentWorkflow.Id)
            {
                DemandAdjustmentBusinessService.CreateDemandAdjustmentPreHistory(instanceId, state);
            }
        }

        public void CreateWorkflowIfNotExists(WorkflowRuntime runtime, Guid instanceId)
        {
            var workflowState = WorkflowStateService.GetWorkflowState(instanceId);

            CreateWorkflowIfNotExists(runtime, instanceId, workflowState.Type);
        }

        public bool CreateWorkflowIfNotExists(WorkflowRuntime runtime, Guid instanceId, WorkflowType workflowType)
        {
            return CreateWorkflowIfNotExists(runtime, instanceId, workflowType, null);
        }

        public bool CreateWorkflowIfNotExists(WorkflowRuntime runtime, Guid instanceId, WorkflowType workflowType,Dictionary<string,object> parameters)
        {
            try
            {
                var workflow = runtime.GetWorkflow(instanceId);
                return true;
            }
            catch (Exception)
            {
            }

            WorkflowInstance instance = null;

            var wfparameters = new Dictionary<string, object>();
            
            if (parameters != null)
                wfparameters = new Dictionary<string, object>(1) {{"WorkflowPersistanceParameters", parameters}};

            if (workflowType == WorkflowType.BillDemandWorkfow)
                instance = runtime.CreateWorkflow(typeof(BillDemand), wfparameters,
                                                  instanceId);
            else if (workflowType == WorkflowType.DemandAdjustmentWorkflow)
                instance = runtime.CreateWorkflow(typeof(DemandAdjustment), wfparameters,
                                                  instanceId);
            else if (workflowType == WorkflowType.DemandWorkflow)
                instance = runtime.CreateWorkflow(typeof(Demand), wfparameters,
                                                  instanceId);
            else
                throw new InvalidOperationException("Невозможно определить тип маршрута");

            instance.Start();

            return false;

            //TODO Синхронизация и класс синхронизатор
        }

        public event EventHandler<SetWorkflowInternalParametersEventArgs> SetInternalParameters;
    }
}
