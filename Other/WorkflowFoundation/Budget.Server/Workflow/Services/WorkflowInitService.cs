using System;
using System.Collections.Generic;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;
using Budget2.Server.Workflow.Interface.DataContracts;
using Budget2.Server.Workflow.Interface.FaultContracts;
using Budget2.Server.Workflow.Interface.Services;
using Budget2.Workflow;
using Common.WF;
using Microsoft.Practices.CompositeWeb;
using System.Linq;


namespace Budget2.Server.Workflow.Services
{
    public class WorkflowInitService : IWorkflowInitService
    {
        public event EventHandler<WorkflowEventArgsWithInitiator> StartProcessing;
        public event EventHandler<WorkflowEventArgsWithInitiator> Sighting;

        public event EventHandler<DenialCommandEventArgs> Denial;
        public event EventHandler<DenialCommandEventArgs> DenialByTechnicalCauses;

        [ServiceDependency]
        public IAuthenticationService AuthenticationService { get; set; }

        [ServiceDependency]
        public IDemandNotificationService DemandNotificationService { get; set; }

        [ServiceDependency]
        public IBillDemandBuinessService BillDemandBuinessService { get; set; }

        [ServiceDependency]
        public IDemandAdjustmentBusinessService DemandAdjustmentBusinessService { get; set; }

        [ServiceDependency]
        public IWorkflowStateService WorkflowStateService { get; set; }

        [ServiceDependency]
        public IBillDemandExportService BillDemandExportService { get; set; }

        [ServiceDependency]
        public IDemandBusinessService DemandBusinessService { get; set; }

        [ServiceDependency]
        public IBillDemandNotificationService BillDemandNotificationService { get; set; }

        [ServiceDependency]
        public IWorkflowParcelService WorkflowParcelService { get; set; }


        private WorkflowRuntime _runtime;

        private volatile object _sync = new object();

        [ServiceDependency]
        public IBillDemandWorkflowService BillDemandWorkflowService
        {
            get;
            set;
        }

        public WorkflowRuntime Runtime
        {
            get
            {
                if (_runtime == null)
                {
                    lock (_sync)
                    {
                        if (_runtime == null)
                        {
                            _runtime = Budget2WorkflowRuntime.Runtime;
                            Budget2WorkflowRuntime.AddExternalDataExchangeService(BillDemandWorkflowService);
                            Budget2WorkflowRuntime.AddExternalDataExchangeService(this);
                            _runtime.AddService(BillDemandBuinessService);
                            _runtime.AddService(DemandAdjustmentBusinessService);
                            _runtime.AddService(BillDemandExportService);
                            _runtime.AddService(DemandBusinessService);
                            _runtime.AddService(BillDemandNotificationService);
                            _runtime.AddService(WorkflowParcelService);
                            _runtime.AddService(DemandNotificationService);
                        }
                    }
                }

                return _runtime;
            }
        }

        public void CreateWorkflowIfNotExists(Guid instanceId)
        {
            var workflowState = WorkflowStateService.GetWorkflowState(instanceId);

            CreateWorkflowIfNotExists(instanceId, workflowState.Type);
        }


        private bool CreateWorkflowIfNotExists(Guid instanceId, WorkflowType workflowType)
        {
            try
            {
                var workflow = Runtime.GetWorkflow(instanceId);
                return true;
            }
            catch (Exception)
            {
            }

            WorkflowInstance instance = null;

            if (workflowType == WorkflowType.BillDemandWorkfow)
                instance = Runtime.CreateWorkflow(typeof (BillDemand), new Dictionary<string, object>(),
                                                  instanceId);
            else if (workflowType == WorkflowType.DemandAdjustmentWorkflow)
                instance = Runtime.CreateWorkflow(typeof (DemandAdjustment), new Dictionary<string, object>(),
                                                  instanceId);
            else if (workflowType == WorkflowType.DemandWorkflow)
                instance = Runtime.CreateWorkflow(typeof (Demand), new Dictionary<string, object>(),
                                                  instanceId);
            else
                throw new InvalidOperationException("Невозможно определить тип маршрута");

            instance.Start();

            return false;

            //TODO Синхронизация и класс синхронизатор
        }

        public void TryLoadWorkflow(Guid instanceId)
        {
            try
            {
                var workflow = Runtime.GetWorkflow(instanceId);
                return;
            }
            catch (Exception)
            {
            }
        }

        public void SetWorkflowState(Guid instanceId, string stateName, string comment)
        {
            var workflowState = WorkflowStateService.GetWorkflowState(instanceId);



            if (workflowState.WorkflowStateName != stateName)
            {
                WorkflowParcelService.AddParcel(instanceId,
                                                new WorkflowSetStateParcel()
                                                    {
                                                        Comment = comment,
                                                        InitiatorId = AuthenticationService.GetCurrentIdentity().Id,
                                                        Command = WorkflowCommand.Unknown,
                                                        PreviousWorkflowState = workflowState
                                                    });
                 using (var sync =  new WorkflowSync(Runtime, instanceId))
                 {
                     if (!CreateWorkflowIfNotExists(instanceId, workflowState.Type)) //Это ожидание создания воркфлоу
                        sync.WaitHandle.WaitOne(600000);

                 }
                var instance = new StateMachineWorkflowInstance(Runtime, instanceId);
                var newWorkflowState = WorkflowStateService.GetWorkflowState(instanceId);
                if (newWorkflowState.WorkflowStateName != stateName)
                {
                    using (var sync = new WorkflowSync(Runtime, instanceId))  //Это ожидание завершения установки состояния воркфлоу
                    {
                        instance.SetState(stateName);
                        sync.WaitHandle.WaitOne(600000);
                    }
                }
                WorkflowState state =
                    WorkflowState.AllStates.First(
                        ws => ws.WorkflowStateName == stateName && ws.Type.Id == workflowState.Type.Id);
                if (!state.IsFinal && !state.IsInitial)
                     RewriteWorkflow(instanceId, state);
            }
        }

        private void RewriteWorkflow(Guid instanceId, WorkflowState state)
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

        #region Общие события На маршрут, Отказ, Отказ по ТП, Утвердить

        public void RaiseStartProcessing(Guid instanceId)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            StartProcessing(null, new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseSighting(Guid instanceId)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            Sighting(null, new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseSighting(Guid instanceId, ServiceIdentity serviceIdentity)
        {
            Sighting(null, new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseDenial(Guid instanceId, string comment)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            var args = new DenialCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { Comment = comment };
            Denial(null, args);
        }

        public void RaiseDenialByTechnicalCauses(Guid instanceId, string comment)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            var args = new DenialCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { Comment = comment };
            DenialByTechnicalCauses(null, args);
        }

        public void RaisePostingAccounting(Guid instanceId)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaisePostingAccounting(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseCheckStatus(Guid instanceId)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaiseCheckStatus(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseSetPaidStatus(Guid instanceId, DateTime? paymentDate, string documentNumber)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaiseSetPaidStatus(new PaidCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { DocumentNumber = documentNumber, PaymentDate = paymentDate });
        }

        public void RaiseSetDenialStatus(Guid instanceId, string comment)
        {
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            var args = new DenialCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { Comment = comment };
            BillDemandWorkflowService.RaiseSetDenialStatus(args);
        }

        public void RaiseExport(Guid instanceId)
        {
            if (!BillDemandBuinessService.IsBillDemandFilialSupportExport(instanceId))
                throw new ImpossibleToExecuteCommandException("Отправка документа в систему ФУС БОСС невозможна, т.к. в договоре контрагент заказчик указан не наш банк!");
            TryLoadWorkflow(instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaiseExport(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseExport(Guid instanceId, ServiceIdentity serviceIdentity)
        {
            BillDemandWorkflowService.RaiseExport(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        #endregion
    }
}
