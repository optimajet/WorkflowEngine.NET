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
        private class MultipleExportState
        {
            public Guid InstanceId { get; set; }

            public MultiThreadedPersistance<CommandExecutionStatus> States { get; set; }

            public AutoResetEvent WaitHandle { get; set; }

            public ServiceIdentity Identity;

            public IAuthorizationService AuthorizationService { get; set; }

            public string Comment { get; set; }

            public WorkflowCommandType CommandToExecute { get; set; }

            public string StateToSet { get; set; }
        }

        private class MultipleExportBillDemandState : MultipleExportState
        {
            public IBillDemandBuinessService BillDemandBusinessService { get; set; }

        }

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

        [ServiceDependency]
        public IWorkflowSupportService WorkflowSupportService { get; set; }


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

        public void Rollback(ApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return;

            if (!AuthorizationService.IsAllowedToExecuteCommand(arg.InstanceId, WorkflowCommandType.Rollback))
                return;

            FireCommandWithWaiting(arg, delegate(ApiCommandArgument arg1)
                                            {
                                                using (var sync = GetWorkflowSync(arg1))
                                                {
                                                    WorkflowInitService.RaiseRollback(arg1.InstanceId, arg1.Comment);
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

                var state = new MultipleExportBillDemandState { InstanceId = instanceId, States = states, WaitHandle = handle, BillDemandBusinessService = BillDemandBuinessService, Identity = AuthenticationService.GetCurrentIdentity(),Comment = arg.Comment};

                ThreadPool.QueueUserWorkItem(RaiseBillDemandExportTask, state);
            }

            WaitHandle.WaitAll(handles.ToArray(), new TimeSpan(0, 0, 10, 0));

            return states.Items;
        }

        public IEnumerable<CommandExecutionStatus> MassExecuteCommand(ApiMassCommandEventArg arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            return !AuthenticationService.IsAuthenticated() ? GetErrorExecutionStatuses(arg) : ExecuteMassCommand(arg);
        }

        public IEnumerable<CommandExecutionStatus> MassExecuteSetState(ApiMassSetStateCommandEventArg arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            return CheckNotAllowToSetState(arg) ? GetErrorExecutionStatuses(arg) : ExecuteMassCommand(arg);
        }

        private IEnumerable<CommandExecutionStatus> ExecuteMassCommand(IMassCommand arg)
        {
            var states = new MultiThreadedPersistance<CommandExecutionStatus>();

            var handles = new List<WaitHandle>();

            foreach (var instanceId in arg.InstanceIds)
            {
                var handle = new AutoResetEvent(false);

                handles.Add(handle);

                var state = new MultipleExportState
                                {
                                    InstanceId = instanceId,
                                    States = states,
                                    WaitHandle = handle,
                                    AuthorizationService = AuthorizationService,
                                    Identity = AuthenticationService.GetCurrentIdentity(),
                                    Comment = arg.Comment,
                                    CommandToExecute = arg.Command,
                                    StateToSet =
                                        (arg is IContainsStateName) ? (arg as IContainsStateName).StateNameToSet : null
                                };

                ThreadPool.QueueUserWorkItem(RaiseCommandTask, state);
            }

            WaitHandle.WaitAll(handles.ToArray(), new TimeSpan(0, 0, 10, 0));

            return states.Items;
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
            if (CheckNotAllowToSetState(arg)) return;

            WorkflowInitService.SetWorkflowState(arg.InstanceId, arg.StateNameToSet, arg.Comment);
        }

        private bool CheckNotAllowToSetState(SetStateApiCommandArgument arg)
        {
            AuthenticationService.Authenticate(arg.SecurityToken);

            if (!AuthenticationService.IsAuthenticated())
                return true;

            if (
                !SecurityEntityService.CheckTrusteeWithIdIsInRole(AuthenticationService.GetCurrentIdentity().Id,
                                                                  BudgetRole.FullControl))
            {
                if (SecurityEntityService.GetAlPermissionsForTrusteeAndworkflow(arg.SecurityToken)
                        .Count(
                            p =>
                            p.LinkedStateToSet != null && p.LinkedStateToSet.WorkflowStateName == arg.StateNameToSet) < 1)
                    return true;
            }
            return false;
        }


        private void RaiseCommandTask(object _multipleExportState)
        {
            var multipleExportState = (MultipleExportState)(_multipleExportState);

            try
            {
                Thread.CurrentPrincipal = new GenericPrincipal(multipleExportState.Identity, new string[] { });
                TryRaiseCommand(multipleExportState);
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


        private void TryRaiseCommand(MultipleExportState multipleExportState)
        {
            if (multipleExportState.CommandToExecute != WorkflowCommandType.SetWorkflowState &&
              !multipleExportState.AuthorizationService.IsAllowedToExecuteCommand(multipleExportState.InstanceId,
                                                                                  multipleExportState.CommandToExecute))
                AddErrorResult(multipleExportState);
            else
            {
                try
                {
                    switch (multipleExportState.CommandToExecute)
                    {
                        case WorkflowCommandType.Sighting:
                            RaiseAction(multipleExportState, WorkflowInitService.RaiseSighting); 
                            break;
                        case WorkflowCommandType.StartProcessing:
                            WorkflowInitService.CreateWorkflowIfNotExists(multipleExportState.InstanceId);
                            using (var sync = GetWorkflowSync(multipleExportState.InstanceId))
                            {
                                sync.WaitHandle.WaitOne(4000);
                            }
                            RaiseAction(multipleExportState, WorkflowInitService.RaiseStartProcessing); 
                            break;
                        case WorkflowCommandType.Denial:
                            RaiseActionWithComment(multipleExportState, WorkflowInitService.RaiseDenial);
                            break;
                        case WorkflowCommandType.Rollback:
                            RaiseActionWithComment(multipleExportState, WorkflowInitService.RaiseRollback); 
                            break;
                        case WorkflowCommandType.SetWorkflowState:
                            RaiseSetStateAction(multipleExportState);
                            break;
                        default:
                            throw new ActionNotSupportedException();
                    }

                }
                catch (Exception ex)
                {
                    AddErrorResult(multipleExportState);
                    throw;
                }

                AddOkResult(multipleExportState);

            }
        }

        private void RaiseActionWithComment(MultipleExportState multipleExportState, Action<Guid, ServiceIdentity, string> action)
        {
            FireCommandWithWaiting(multipleExportState.InstanceId, delegate(Guid arg1)
            {
                using (var sync = GetWorkflowSync(arg1))
                {
                    action.Invoke(arg1, multipleExportState.Identity, multipleExportState.Comment);
                    sync.WaitHandle.WaitOne(60000);
                }
            }, arg2 => arg2);
        }


        private void RaiseSetStateAction(MultipleExportState multipleExportState)
        {
            FireCommandWithWaiting(multipleExportState.InstanceId, delegate(Guid arg1)
            {
                using (var sync = GetWorkflowSync(arg1))
                {
                    WorkflowInitService.SetWorkflowState(arg1, multipleExportState.Identity, multipleExportState.StateToSet, multipleExportState.Comment);
                    sync.WaitHandle.WaitOne(60000);
                }
            }, arg2 => arg2);
        }

        private void RaiseAction(MultipleExportState multipleExportState, Action<Guid, ServiceIdentity> action)
        {
            FireCommandWithWaiting(multipleExportState.InstanceId, delegate(Guid arg1)
            {
                using (var sync = GetWorkflowSync(arg1))
                {
                    action.Invoke(arg1, multipleExportState.Identity);
                    sync.WaitHandle.WaitOne(60000);
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
                    RaiseExportBillDemand(multipleExportBillDemandState);

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

        private void RaiseExportBillDemand(MultipleExportBillDemandState multipleExportBillDemandState)
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


        private void FireCommandWithWaiting(ApiCommandArgument arg, Action<ApiCommandArgument> action)
        {
            FireCommandWithWaiting<ApiCommandArgument>(arg, action, arg1 => arg1.InstanceId);
        }


        private void FireCommandWithWaiting<T>(T arg, Action<T> action, Func<T, Guid> instanceIdGetter)
        {
            try
            {
                action.Invoke(arg);
            }
            catch (ImpossibleToExecuteCommandException ex)
            {
                throw new FaultException<BaseFault>(new BaseFault((int)ErrorCodes.Empty),
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
                    throw new FaultException<BaseFault>(new BaseFault((int)ErrorCodes.Empty),
                                                        new FaultReason(ex.Message));
                }
                catch (Exception ex)
                {
                    throw new FaultException<BaseFault>(new BaseFault((int)ErrorCodes.CommandProccessingError), new FaultReason("Не удалось выполнить команду. Попробуйте провести данную операцию еще раз. Если сообщение будет повторятся, обратитесь к Администратору."));
                }

            }
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

        private IEnumerable<CommandExecutionStatus> GetErrorExecutionStatuses(IMassCommand arg)
        {
            return arg.InstanceIds.Select(
                p => new CommandExecutionStatus { InstanceId = p, Result = CommandExecutionResult.Error });
        }
    }
}
