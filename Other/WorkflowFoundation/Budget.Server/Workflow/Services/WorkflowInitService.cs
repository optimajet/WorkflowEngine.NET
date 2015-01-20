using System;
using System.Collections.Generic;
using System.Reflection;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using Budget2.DAL;
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
using BillDemand = Budget2.Workflow.BillDemand;
using Demand = Budget2.Workflow.Demand;
using DemandAdjustment = Budget2.Workflow.DemandAdjustment;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;


namespace Budget2.Server.Workflow.Services
{
    public class WorkflowInitService : Budget2DataContextService, IWorkflowInitService
    {
        public event EventHandler<WorkflowEventArgsWithInitiator> StartProcessing;
        public event EventHandler<WorkflowEventArgsWithInitiator> Sighting;

        public event EventHandler<DenialCommandEventArgs> Denial;
        public event EventHandler<DenialCommandEventArgs> Rollback;
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

        [ServiceDependency]
        public IWorkflowSupportService WorkflowSupportService { get; set; }


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
                            Budget2WorkflowRuntime.AddExternalDataExchangeService(WorkflowSupportService);
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

        #region Общие события На маршрут, Отказ, Отказ по ТП, Утвердить

        public void RaiseStartProcessing(Guid instanceId)
        {
            RaiseStartProcessing(instanceId,AuthenticationService.GetCurrentIdentity());
        }

        public void RaiseStartProcessing(Guid instanceId, ServiceIdentity serviceIdentity)
        { 
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            StartProcessing(null, new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseSighting(Guid instanceId)
        {
            RaiseSighting(instanceId, AuthenticationService.GetCurrentIdentity());
        }

        public void RaiseSighting(Guid instanceId, ServiceIdentity serviceIdentity)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            Sighting(null, new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseDenial(Guid instanceId, string comment)
        {
            RaiseDenial(instanceId, AuthenticationService.GetCurrentIdentity(),comment);
            
        }

        public void RaiseDenial(Guid instanceId, ServiceIdentity serviceIdentity, string comment)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            var args = new DenialCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { Comment = comment };
            Denial(null, args);
        }

        public void RaiseRollback(Guid instanceId, string comment)
        {
            RaiseRollback(instanceId, AuthenticationService.GetCurrentIdentity(), comment);
        }

        public void RaiseRollback(Guid instanceId, ServiceIdentity serviceIdentity, string comment)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime,instanceId);
            Rollback(null, new DenialCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { Comment = comment });
        }


        public void RaiseDenialByTechnicalCauses(Guid instanceId, string comment)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            var args = new DenialCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { Comment = comment };
            DenialByTechnicalCauses(null, args);
        }

        public void RaisePostingAccounting(Guid instanceId)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaisePostingAccounting(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseCheckStatus(Guid instanceId)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaiseCheckStatus(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseSetPaidStatus(Guid instanceId, DateTime? paymentDate, string documentNumber)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaiseSetPaidStatus(new PaidCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { DocumentNumber = documentNumber, PaymentDate = paymentDate });
        }

        public void RaiseSetDenialStatus(Guid instanceId, string comment)
        {
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            var args = new DenialCommandEventArgs(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId) { Comment = comment };
            BillDemandWorkflowService.RaiseSetDenialStatus(args);
        }

        public void RaiseExport(Guid instanceId)
        {
            if (!BillDemandBuinessService.IsBillDemandSupportExport(instanceId))
                throw new ImpossibleToExecuteCommandException("Отправка документа в систему \"ФУС БОСС\" невозможна, т.к. в договоре контрагент заказчик указан не наш банк!");
            WorkflowSupportService.UpgradeWorkflow(Runtime, instanceId);
            var serviceIdentity = AuthenticationService.GetCurrentIdentity();
            BillDemandWorkflowService.RaiseExport(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void RaiseExport(Guid instanceId, ServiceIdentity serviceIdentity)
        {
            BillDemandWorkflowService.RaiseExport(new WorkflowEventArgsWithInitiator(instanceId, serviceIdentity.Id, serviceIdentity.ImpersonatedId));
        }

        public void CreateWorkflowIfNotExists(Guid instanceId)
        {
            WorkflowSupportService.CreateWorkflowIfNotExists(Runtime, instanceId);
        }

        public void SetWorkflowState(Guid instanceId, string stateName, string comment)
        {
            SetWorkflowState(instanceId, AuthenticationService.GetCurrentIdentity(),stateName,comment);
        }

        public void SetWorkflowState(Guid instanceId, ServiceIdentity serviceIdentity, string stateName, string comment)
        {
            var workflowState = WorkflowStateService.GetWorkflowState(instanceId);


            if (workflowState.WorkflowStateName != stateName || workflowState.IsInitial)
            //Для черновиков устанавливаем всегда это временно
            {
                WorkflowSupportService.TryUpgradeWorkflow(Runtime, instanceId);

                WorkflowParcelService.AddParcel(instanceId,
                                                new WorkflowSetStateParcel()
                                                {
                                                    Comment = comment,
                                                    InitiatorId = serviceIdentity.Id,
                                                    Command = WorkflowCommand.Unknown,
                                                    PreviousWorkflowState = workflowState
                                                });
                bool isIdled = true;
                using (var sync = new WorkflowSync(Runtime, instanceId))
                {
                    if (!WorkflowSupportService.CreateWorkflowIfNotExists(Runtime, instanceId, workflowState.Type))
                    //Это ожидание создания воркфлоу
                    {
                        sync.WaitHandle.WaitOne(600000);
                        isIdled = sync.WasIdled;
                    }

                }
                //Если воркфлоу не стало идленым - то его необходимо удалить полностью и создать заново
                if (!isIdled)
                {
                    using (var context = this.CreateContext())
                    {
                        context.DeleteWorkflowInPesistenceStore(instanceId);
                        context.SubmitChanges();
                    }
                    using (var sync = new WorkflowSync(Runtime, instanceId))
                    {
                        if (!WorkflowSupportService.CreateWorkflowIfNotExists(Runtime, instanceId, workflowState.Type))
                        //Это ожидание создания воркфлоу
                        {
                            sync.WaitHandle.WaitOne(600000);
                        }

                    }
                }

                var instance = new StateMachineWorkflowInstance(Runtime, instanceId);
                var newWorkflowState = WorkflowStateService.GetWorkflowState(instanceId);
                if (newWorkflowState.WorkflowStateName != stateName)
                {
                    using (var sync = new WorkflowSync(Runtime, instanceId))
                    //Это ожидание завершения установки состояния воркфлоу
                    {
                        instance.SetState(stateName);
                        sync.WaitHandle.WaitOne(600000);
                    }
                }
                WorkflowState state =
                    WorkflowState.AllStates.First(
                        ws => ws.WorkflowStateName == stateName && ws.Type.Id == workflowState.Type.Id);
                if (!state.IsFinal && !state.IsInitial)
                    WorkflowSupportService.RewriteWorkflow(instanceId, state);
                //Для РД удаляем историю согласования
                if (workflowState.Type == WorkflowType.BillDemandWorkfow)
                {
                    if (state == WorkflowState.BillLimitManagerSighting)
                    {
                        BillDemandBuinessService.LimitExecutorResetSights(instanceId);
                    }

                    BillDemandBuinessService.LimitExecutorResetSights(instanceId);
                    BillDemandBuinessService.LimitManagerResetSights(instanceId);
                }

            }
        }

        #endregion
    }
}
