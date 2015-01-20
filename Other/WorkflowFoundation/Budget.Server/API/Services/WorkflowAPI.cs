using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;
using System.Workflow.Activities;
using Budget2.DAL.DataContracts;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.API.Interface.Faults;
using Budget2.Server.API.Interface.Services;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;
using Budget2.Server.Workflow.Interface.FaultContracts;
using Budget2.Server.Workflow.Interface.Services;
using Common.Utils;
using Common.WCF;
using Common.WF;
using Microsoft.Practices.CompositeWeb;

namespace Budget2.Server.API.Services
{
    //TODO FAULTS!!!!!!!!!!
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]

    public class WorkflowAPI : IWorkflowApi
    {
        public WorkflowAPI()
        {
            try
            {
                 WebClientApplication.BuildItemWithCurrentContext(this);
            }
            catch (NullReferenceException)
            {
            }
           
        }

        [ServiceDependency]
        public IAuthenticationService AuthenticationService { get; set; }

        [ServiceDependency]
        public IAuthorizationService AuthorizationService { get; set; }

        [ServiceDependency]
        public IWorkflowInitService WorkflowInitService { get; set; }

        [ServiceDependency]
        public IWorkflowStateService WorkflowStateService { get; set; }

        [ServiceDependency]
        public IBillDemandWorkflowService BillDemandWorkflowService { get; set; }

        [ServiceDependency]
        public IBillDemandBuinessService BillDemandBuinessService { get; set; }

        [ServiceDependency]
        public ISecurityEntityService SecurityEntityService { get; set; }

        [ServiceDependency]
        public IWorkflowParcelService WorkflowParcelService { get; set; }


        public void Sighting(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.Sighting))
                return;

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
                                            {
                                                using (var sync = GetWorkflowSync(arg1))
                                                {
                                                    WorkflowInitService.RaiseSighting(arg1.InstanceId);
                                                    sync.WaitHandle.WaitOne(4000);
                                                }
                                            });

        }

        private WorkflowSync GetWorkflowSync(ApiCommandArgument arg)
        {
            return GetWorkflowSync(arg.InstanceId);
        }

        private WorkflowSync GetWorkflowSync(Guid instanceId)
        {
            var sync = new WorkflowSync(WorkflowInitService.Runtime, instanceId);
            sync.WorkflowIdled += delegate(object sender, EventArgs e)
                                      {
                                          try
                                          {
                                              var wf = WorkflowInitService.Runtime.GetWorkflow(instanceId);
                                              //wf.TryUnload();
                                          }
                                          catch (InvalidOperationException)
                                          {
                                          }

                                      };

            return sync;
        }


        public void Denial(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.Denial))
                return;

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
                                            {
                                                using (var sync = GetWorkflowSync(arg1))
                                                {
                                                    WorkflowInitService.RaiseDenial(arg1.InstanceId, arg1.Comment);
                                                    sync.WaitHandle.WaitOne(4000);
                                                }
                                            });

        }

        private void FireCommandWithWaiting<T>(T arg, Action<T> action, Func<T, Guid> instanceIdGetter)
        {
            try
            {
                action.Invoke(arg);
            }
            catch (ImpossibleToExecuteCommandException ex)
            {
                throw new FaultException<BaseFault>(new BaseFault((int)ErrorCodes.CommandProccessingError),
                                                    new FaultReason(ex.Message));
            }
            catch (EventDeliveryFailedException)
            {
                using (var sync = GetWorkflowSync(instanceIdGetter.Invoke(arg)))
                {
                    sync.WaitHandle.WaitOne(2000);
                }

                try
                {
                    action.Invoke(arg);
                }
                catch (ImpossibleToExecuteCommandException ex)
                {
                    throw new FaultException<BaseFault>(new BaseFault((int) ErrorCodes.CommandProccessingError),
                                                        new FaultReason(ex.Message));
                }
                catch (Exception)
                {
                    throw new FaultException<BaseFault>(new BaseFault((int)ErrorCodes.CommandProccessingError), new FaultReason("Не удалось выполнить команду. Попробуйте провести данную операцию еще раз. Если сообщение будет повторятся, обратитесь к Администратору."));
                }

            }
        }

        private void FireCommandWithWaiting(ApiCommandArgument arg, Action<ApiCommandArgument> action)
        {
            FireCommandWithWaiting<ApiCommandArgument>(arg, action, arg1 => arg1.InstanceId);
        }

        private void FireCommandWithWaiting(BillDemandPaidApiCommandArgument arg, Action<BillDemandPaidApiCommandArgument> action)
        {
            try
            {
                action.Invoke(arg);
            }
            catch (EventDeliveryFailedException)
            {
                using (var sync = GetWorkflowSync(arg))
                {
                    sync.WaitHandle.WaitOne(2000);
                }

                try
                {
                    action.Invoke(arg);
                }
                catch (Exception)
                {
                    throw new FaultException<BaseFault>(new BaseFault((int)ErrorCodes.CommandProccessingError), new FaultReason("Не удалось выполнить команду. Попробуйте провести данную операцию еще раз. Если сообщение будет повторятся, обратитесь к Администратору."));
                }
            }
        }


        public void DenialByTechnicalCauses(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.DenialByTechnicalCauses))
                return;

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
            {
                using (var sync = GetWorkflowSync(arg1))
                {
                    WorkflowInitService.RaiseDenialByTechnicalCauses(arg1.InstanceId, arg1.Comment);
                    sync.WaitHandle.WaitOne(4000);
                }
            });
        }

        public void StartProcessing(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.StartProcessing))
                return;

            WorkflowInitService.CreateWorkflowIfNotExists(arg.InstanceId);

            using (var sync = GetWorkflowSync(arg))
            {
                sync.WaitHandle.WaitOne(4000);
            }

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
                                            {
                                                using (var sync = GetWorkflowSync(arg1))
                                                {
                                                    WorkflowInitService.RaiseStartProcessing(arg1.InstanceId);
                                                    sync.WaitHandle.WaitOne(4000);
                                                }
                                            });

        }

        public void PostingAccounting(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.PostingAccounting))
                return;

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
            {
                using (var sync = GetWorkflowSync(arg1))
                {
                    WorkflowInitService.RaisePostingAccounting(arg1.InstanceId);
                    sync.WaitHandle.WaitOne(4000);
                }
            });
        }

        public void CheckStatus(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.CheckStatus))
                return;


            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
                                            {
                                                using (var sync = GetWorkflowSync(arg1))
                                                {
                                                    WorkflowInitService.RaiseCheckStatus(arg1.InstanceId);
                                                    sync.WaitHandle.WaitOne(4000);
                                                }
                                            });

        }

        public void SetPaidStatus(BillDemandPaidApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.SetPaidStatus))
                return;

            FireCommandWithWaiting(arg, delegate(BillDemandPaidApiCommandArgument arg1)
                                            {
                                                using (var sync = GetWorkflowSync(arg1))
                                                {
                                                    WorkflowInitService.RaiseSetPaidStatus(arg1.InstanceId,
                                                                                           arg1.PaymentDate,
                                                                                           arg1.DocumentNumber);
                                                    sync.WaitHandle.WaitOne(4000);
                                                }
                                            });

        }

        public void SetDenialStatus(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.SetDenialStatus))
                return;

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
            {
                using (var sync = GetWorkflowSync(arg1))
                {
                    WorkflowInitService.RaiseSetDenialStatus(arg1.InstanceId, arg1.Comment);
                    sync.WaitHandle.WaitOne(4000);
                }
            });
        }

        public void ExportBillDemand(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.Export))
                return;

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
            {
                using (var sync = GetWorkflowSync(arg1))
                {
                    WorkflowInitService.RaiseExport(arg1.InstanceId);
                    sync.WaitHandle.WaitOne(60000);
                }
            });

            var state = WorkflowStateService.GetCurrentState(arg.InstanceId);
            if (state == WorkflowState.BillDemandInAccountingWithExport)
            {
                var errorMessage = WorkflowParcelService.GetAndRemoveMessage(arg.InstanceId);

                throw new FaultException<BaseFault>(new BaseFault((int)ErrorCodes.BillDemandUploadError),
                                                    new FaultReason(string.IsNullOrEmpty(errorMessage) ?
                                                        "Не удалось выгрузить расходный документ. Попробуйте провести данную операцию еще раз. Если сообщение будет повторятся, обратитесь к Администратору." : errorMessage));
            }
        }

        private class MultipleExportState
        {
            public Guid InstanceId { get; set; }

            public MultiThreadedPersistance<CommandExecutionStatus> States { get; set; }

            public AutoResetEvent WaitHandle { get; set; }

            public ServiceIdentity Identity;

            public IAuthorizationService AuthorizationService { get; set; }
        }

        private class MultipleExportBillDemandState : MultipleExportState
        {
            public IBillDemandBuinessService BillDemandBusinessService { get; set; }

        }

        public IEnumerable<CommandExecutionStatus> MassExportBillDemands(ApiMassCommandEventArg arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return GetErrorExecutionStatuses(arg);

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceIds, WorkflowCommandType.Export))
                GetErrorExecutionStatuses(arg);

            var states = new MultiThreadedPersistance<CommandExecutionStatus>();

            var handles = new List<WaitHandle>();

            foreach (var instanceId in arg.InstanceIds)
            {
                var handle = new AutoResetEvent(false);

                handles.Add(handle);

                var state = new MultipleExportBillDemandState { InstanceId = instanceId, States = states, WaitHandle = handle, BillDemandBusinessService = BillDemandBuinessService, Identity = AuthenticationService.GetCurrentIdentity() };

                ThreadPool.QueueUserWorkItem(RaiseBillDemandExportTask, state);
            }

            WaitHandle.WaitAll(handles.ToArray(), new TimeSpan(0, 0, 10, 0));

            return states.Items;
        }

        public IEnumerable<CommandExecutionStatus> MassApproveDemands(ApiMassCommandEventArg arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return GetErrorExecutionStatuses(arg);

            var states = new MultiThreadedPersistance<CommandExecutionStatus>();

            var handles = new List<WaitHandle>();

            foreach (var instanceId in arg.InstanceIds)
            {
                var handle = new AutoResetEvent(false);

                handles.Add(handle);

                var state = new MultipleExportState { InstanceId = instanceId, States = states, WaitHandle = handle, AuthorizationService = AuthorizationService, Identity = AuthenticationService.GetCurrentIdentity() };

                ThreadPool.QueueUserWorkItem(RaiseDemandSightingTask, state);
            }

            WaitHandle.WaitAll(handles.ToArray(), new TimeSpan(0, 0, 10, 0));

            return states.Items;
        }

        private void RaiseDemandSightingTask(Object _multipleExportState)
        {
            var multipleExportState = (MultipleExportState)(_multipleExportState);

            try
            {
                Thread.CurrentPrincipal = new GenericPrincipal(multipleExportState.Identity, new string[] {});
                TryRiseDemandSighting(multipleExportState);
            }
            catch
            {
                AddErrorResult(multipleExportState);
                throw;
            }
            finally
            {
                Thread.CurrentPrincipal = null;
                
                multipleExportState.WaitHandle.Set();
            }
        }

        private void TryRiseDemandSighting(MultipleExportState multipleExportState)
        {

            if (
                !multipleExportState.AuthorizationService.IsAllowedToExecuteCommand(multipleExportState.InstanceId,
                                                                                    WorkflowCommandType.Sighting))
                AddErrorResult(multipleExportState);
            else
            {
                try
                {
                    RaiseDemandSighting(multipleExportState);
                }
                catch (Exception ex)
                {
                    AddErrorResult(multipleExportState);
                    throw;
                }

                AddOkResult(multipleExportState);

            }
        }

        private void RaiseDemandSighting(MultipleExportState multipleExportState)
        {
            FireCommandWithWaiting(multipleExportState.InstanceId, delegate(Guid arg1)
                                                                       {
                                                                           using (
                                                                               var sync =
                                                                                   GetWorkflowSync(
                                                                                       arg1))
                                                                           {
                                                                               WorkflowInitService.
                                                                                   RaiseSighting(arg1,
                                                                                                 multipleExportState
                                                                                                     .Identity);
                                                                               sync.WaitHandle.WaitOne
                                                                                   (60000);
                                                                           }
                                                                       }, arg2 => arg2);
        }

        private void RaiseBillDemandExportTask(Object _multipleExportState)
        {
            var multipleExportState = (MultipleExportBillDemandState)(_multipleExportState);

            try
            {
                TryExportBillDemand(multipleExportState);

            }
            finally
            {

                multipleExportState.WaitHandle.Set();
            }
        }

        private void TryExportBillDemand(MultipleExportBillDemandState multipleExportBillDemandState)
        {
            if (!multipleExportBillDemandState.BillDemandBusinessService.CheckPaymentPlanFilled(multipleExportBillDemandState.InstanceId))
            {
                AddPaymentPlanNotSelectedResult(multipleExportBillDemandState);
            }
            else
            {
                try
                {
                    FireExportBillDemand(multipleExportBillDemandState);

                    if (WorkflowStateService.GetCurrentState(multipleExportBillDemandState.InstanceId) == WorkflowState.BillDemandInAccountingWithExport)
                        AddErrorResult(multipleExportBillDemandState);
                    else
                        AddOkResult(multipleExportBillDemandState);
                }
                catch (Exception ex)
                {
                    AddErrorResult(multipleExportBillDemandState);
                }
            }
        }

        private void FireExportBillDemand(MultipleExportBillDemandState multipleExportBillDemandState)
        {
            FireCommandWithWaiting(multipleExportBillDemandState.InstanceId, delegate(Guid arg1)
                                                                       {
                                                                           using (
                                                                               var sync =
                                                                                   GetWorkflowSync(
                                                                                       arg1))
                                                                           {
                                                                               WorkflowInitService.
                                                                                   RaiseExport(arg1, multipleExportBillDemandState.Identity);
                                                                               sync.WaitHandle.WaitOne
                                                                                   (60000);
                                                                           }
                                                                       }, arg2 => arg2);
        }

        private void AddOkResult(MultipleExportState multipleExportState)
        {
            multipleExportState.States.AddItem(new CommandExecutionStatus()
                                                   {
                                                       InstanceId = multipleExportState.InstanceId,
                                                       Result = CommandExecutionResult.OK
                                                   });
        }

        private void AddErrorResult(MultipleExportState multipleExportState)
        {
            multipleExportState.States.AddItem(new CommandExecutionStatus()
                                                   {
                                                       InstanceId = multipleExportState.InstanceId,
                                                       Result = CommandExecutionResult.Error
                                                   });
        }

        private void AddPaymentPlanNotSelectedResult(MultipleExportBillDemandState multipleExportBillDemandState)
        {
            multipleExportBillDemandState.States.AddItem(new CommandExecutionStatus()
                                                   {
                                                       InstanceId = multipleExportBillDemandState.InstanceId,
                                                       Result = CommandExecutionResult.BillDemandExportPaymentPlanNotSelected
                                                   });
        }


        private IEnumerable<CommandExecutionStatus> GetErrorExecutionStatuses(ApiMassCommandEventArg arg)
        {
            return arg.InstanceIds.Select(
                p => new CommandExecutionStatus { InstanceId = p, Result = CommandExecutionResult.Error });
        }


        public List<WorkflowCommandType> GetListOfAllowedOperations(ApiCommandArgument arg)
        {
            var allowedOperations = new List<WorkflowCommandType>();

            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return allowedOperations;

            var commandsToCheck = new List<WorkflowCommandType>()
                                      {
                                          WorkflowCommandType.StartProcessing,
                                          WorkflowCommandType.Sighting,
                                          WorkflowCommandType.Denial,
                                          WorkflowCommandType.DenialByTechnicalCauses,
                                          WorkflowCommandType.PostingAccounting,
                                          WorkflowCommandType.CheckStatus,
                                          WorkflowCommandType.SetDenialStatus,
                                          WorkflowCommandType.SetPaidStatus,
                                          WorkflowCommandType.Export
                                      };

          allowedOperations = AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, commandsToCheck);

          if (!SecurityEntityService.CheckTrusteeWithIdIsInRole(AuthenticationService.GetCurrentIdentity().Id, BudgetRole.FullControl))
          {
              var type = WorkflowStateService.TryGetExpectedWorkflowType(arg.InstanceId);
              if (SecurityEntityService.GetAlPermissionsForTrusteeAndworkflow(arg.SecurityToken)
                      .Count(p => p.LinkedStateToSet != null && p.LinkedStateToSet.Type.Id == type.Id ) > 0)
                  allowedOperations.Add(WorkflowCommandType.SetWorkflowState);
          }
          else
          {
              allowedOperations.Add(WorkflowCommandType.SetWorkflowState);
          }
          

            return allowedOperations;
        }

        public List<WorkflowStateInfo> GetAvailiableWorkflowStateToSet(ApiCommandArgument arg)
        {
            var infos = new List<WorkflowStateInfo>();

            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return infos;

             
            if (!SecurityEntityService.CheckTrusteeWithIdIsInRole(AuthenticationService.GetCurrentIdentity().Id, BudgetRole.FullControl))
            {
                var permissions
                    = SecurityEntityService.GetAlPermissionsForTrusteeAndworkflow(arg.SecurityToken)
                        .Where(p => p.LinkedStateToSet != null)
                        .Select(p => p.LinkedStateToSet);
             
                if (permissions.Count() < 1)
                    return infos;
                infos = WorkflowStateService.GetAllAvailiableStates(arg.InstanceId)
                                            .Where(i=>permissions.Count(p=>p.WorkflowStateName == i.StateSystemName && p.Type.Id == i.WorkflowTypeId) > 0)
                                            .ToList();
            }
            else
            {
                infos = WorkflowStateService.GetAllAvailiableStates(arg.InstanceId);
            }

            return infos;
        }

        

        public void SetWorkflowState(SetStateApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!SecurityEntityService.CheckTrusteeWithIdIsInRole(AuthenticationService.GetCurrentIdentity().Id, BudgetRole.FullControl))
            {
                if (SecurityEntityService.GetAlPermissionsForTrusteeAndworkflow(arg.SecurityToken)
                        .Count(
                            p =>
                            p.LinkedStateToSet != null && p.LinkedStateToSet.WorkflowStateName == arg.StateNameToSet) < 1)
                    return;
            }

            WorkflowInitService.SetWorkflowState(arg.InstanceId, arg.StateNameToSet, arg.Comment);
        }
    }
}
