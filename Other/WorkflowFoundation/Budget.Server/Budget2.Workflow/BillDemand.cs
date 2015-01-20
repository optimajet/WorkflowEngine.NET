using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Budget2.DAL.DataContracts;
using System;
using System.Workflow.Activities;
using Budget2.Server.Business.Interface.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Workflow.Interface.DataContracts;
using Common;
using System.Linq;

namespace Budget2.Workflow
{
    public sealed partial class BillDemand : BudgetWorkflow
    {
        public new Dictionary<string, object> WorkflowPersistanceParameters
        {
            set
            {
                base.WorkflowPersistanceParameters = value;
            }
            get
            {
                return base.WorkflowPersistanceParameters;
            }

        }

        private const string LastExternalStatusPersistanceKey = "{DCCC371D-5BE4-4BFB-9B96-665EB4B7C1E3}";

        private BillDemandExternalStatus LastExternalStatus
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(LastExternalStatusPersistanceKey))
                    return WorkflowPersistanceParameters[LastExternalStatusPersistanceKey] as BillDemandExternalStatus;
                return BillDemandExternalStatus.Unknown;
            }
            set { WorkflowPersistanceParameters[LastExternalStatusPersistanceKey] = value; }
        }

        private const string IsSupressWriteHistoryPersistanceKey = "{617BDAE8-8FE9-4EE1-855E-4069769FD5C4}";

        private bool IsSupressWriteHistory
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(IsSupressWriteHistoryPersistanceKey))
                    return (bool)WorkflowPersistanceParameters[IsSupressWriteHistoryPersistanceKey];
                return false;
            }
            set { WorkflowPersistanceParameters[IsSupressWriteHistoryPersistanceKey] = value; }
        }


        private const string IsOperativePersistanceKey = "{30FA8DEB-FEAE-487F-A8C5-D43A1391D126}";

        private bool IsOperative
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(IsOperativePersistanceKey))
                    return (bool)WorkflowPersistanceParameters[IsOperativePersistanceKey];
                return false;
            }
            set { WorkflowPersistanceParameters[IsOperativePersistanceKey] = value; }
        }

        public BillDemand()
        {
            InitializeComponent();
        }

        #region Обработчики для множественных согласований

        private void CheckLimitExecutorSight_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            var sightingIdentity = GetSightingIdentity();
            e.Result = Budget2WorkflowRuntime.BillDemandBuinessService.LimitExecutorSight(WorkflowInstanceId, sightingIdentity, TransitionInitiator);

            Budget2WorkflowRuntime.BillDemandBuinessService.LimitManagerSight(WorkflowInstanceId, sightingIdentity, TransitionInitiator, IsOperative);

            if (!e.Result)
                WriteTransitionToHistory(WorkflowState.BillDemandLimitExecutorSighting);
        }

        private void CheckLimitManagerSightingComplete_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            if (!IsOperative)
            {
                var sighters =
                    Budget2WorkflowRuntime.BillDemandBuinessService.GetLimitManagerSighters(WorkflowInstanceId,IsOperative);
                var sightings =
                    Budget2WorkflowRuntime.BillDemandBuinessService.GetLimitManagerSightings(WorkflowInstanceId);
                e.Result =
                    sighters.TrueForAll(
                        p => sightings.Count(s => s.LimitId == p.LimitId && s.SighterId == p.SighterId) > 0);


                if (!e.Result)
                {
                    foreach (LimitSighting t in sightings)
                    {
                        WriteTransitionToHistory(WorkflowState.BillLimitManagerSighting, t.InitiatorId,
                                                 t.SighterId, t.SightingTime);
                    }
                }
                else
                {
                    for (var i = 0; i < sightings.Count; i++)
                    {
                        if (i != sightings.Count - 1)
                            WriteTransitionToHistory(WorkflowState.BillLimitManagerSighting, sightings[i].InitiatorId,
                                                     sightings[i].SighterId, sightings[i].SightingTime);
                        else
                            WriteTransitionToHistory(WorkflowState.BillDemandUPKZCuratorSighting,
                                                     sightings[i].InitiatorId,
                                                     sightings[i].SighterId, sightings[i].SightingTime);
                    }
                }

                IsSupressWriteHistory = e.Result;
            }
            else
            {
                var sighters =
                  Budget2WorkflowRuntime.BillDemandBuinessService.GetLimitManagerSighters(WorkflowInstanceId,IsOperative);
                var sightings =
                    Budget2WorkflowRuntime.BillDemandBuinessService.GetLimitManagerSightings(WorkflowInstanceId);
                e.Result =
                    sighters.TrueForAll(
                        p => sightings.Count(s => s.LimitId == p.LimitId) > 0);
            }

        }

        private Guid GetSightingIdentity()
        {
            return ImpersonatedIdentityId.HasValue ? ImpersonatedIdentityId.Value : TransitionInitiator;
        }

        private void CheckLimitMangerSight_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            var sightingIdentity = GetSightingIdentity();
            e.Result = Budget2WorkflowRuntime.BillDemandBuinessService.LimitManagerSight(WorkflowInstanceId, sightingIdentity, TransitionInitiator, IsOperative);

            if (!e.Result)
                WriteTransitionToHistory(WorkflowState.BillLimitManagerSighting);
        }


        private void ResetLimitExecutorSights_ExecuteCode(object sender, EventArgs e)
        {
            Budget2WorkflowRuntime.BillDemandBuinessService.LimitExecutorResetSights(WorkflowInstanceId);
        }

        private void ResetLimitManagerSights_ExecuteCode(object sender, EventArgs e)
        {
            Budget2WorkflowRuntime.BillDemandBuinessService.LimitManagerResetSights(WorkflowInstanceId);
        }

        #endregion

        #region Проверка по бюджету

        private void CheckBillDemandValue_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            e.Result = Budget2WorkflowRuntime.BillDemandBuinessService.CheckUPKZHeadMustSight(WorkflowInstanceId);
        }


        private void CheckInitiatorHeadMustSign_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            e.Result = Budget2WorkflowRuntime.BillDemandBuinessService.CheckInitiatorHeadMustSign(WorkflowInstanceId);
        }


        #endregion

        #region Инициализация сотояний

        private void DraftInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandDraft);
            Comment = string.Empty;
            PreviousWorkflowState = WorkflowState.BillDemandDraft;
        }


        private void UPKZCntrollerSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            if (!(PreviousWorkflowState.WorkflowStateName == WorkflowState.BillLimitManagerSighting.WorkflowStateName && LastCommand.Id == WorkflowCommand.Sighting.Id))
                WriteTransitionToHistory(WorkflowState.BillDemandUPKZCntrollerSighting);

            PreviousWorkflowState = WorkflowState.BillDemandUPKZCntrollerSighting;
            SendNotifications(WorkflowState.BillDemandUPKZCntrollerSighting);
        }

        private void LimitManagerSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillLimitManagerSighting);
            PreviousWorkflowState = WorkflowState.BillLimitManagerSighting;
            CheckBillDemand(WorkflowState.BillLimitManagerSighting);
            SendNotifications(WorkflowState.BillLimitManagerSighting);
        }

        private void UPKZCuratorSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            if (!IsSupressWriteHistory)
                WriteTransitionToHistory(WorkflowState.BillDemandUPKZCuratorSighting);
            IsSupressWriteHistory = false;
            PreviousWorkflowState = WorkflowState.BillDemandUPKZCuratorSighting;
            SendNotifications(WorkflowState.BillDemandUPKZCuratorSighting);
        }

        private void PostingAccountingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandPostingAccounting);
            PreviousWorkflowState = WorkflowState.BillDemandPostingAccounting;
        }

        private void OnPaymentInitCode_ExecuteCode(object sender, EventArgs e)
        {
            if (PreviousWorkflowState.WorkflowStateName != WorkflowState.BillDemandOnPayment.WorkflowStateName)
            {
                WriteTransitionToHistory(WorkflowState.BillDemandOnPayment);
                Budget2WorkflowRuntime.BillDemandBuinessService.SetTransferDate(WorkflowInstanceId);
            }
            PreviousWorkflowState = WorkflowState.BillDemandOnPayment;

        }

        private void LimitExecutorSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandLimitExecutorSighting);
            PreviousWorkflowState = WorkflowState.BillDemandLimitExecutorSighting;
            CheckBillDemand(WorkflowState.BillDemandLimitExecutorSighting);
            SendNotifications(WorkflowState.BillDemandLimitExecutorSighting);
        }

        private void UPKZHeadSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandUPKZHeadSighting);
            PreviousWorkflowState = WorkflowState.BillDemandUPKZHeadSighting;
            CheckBillDemand(WorkflowState.BillDemandUPKZHeadSighting);
            SendNotifications(WorkflowState.BillDemandUPKZHeadSighting);
        }

        private void HeadInitiatorSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandHeadInitiatorSighting);
            PreviousWorkflowState = WorkflowState.BillDemandHeadInitiatorSighting;
            SendNotifications(WorkflowState.BillDemandHeadInitiatorSighting);
        }


        private void InAccountingWithExportInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandInAccountingWithExport);
            PreviousWorkflowState = WorkflowState.BillDemandInAccountingWithExport;
        }

        private void InitiatorConfirmationInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandInitiatorConfirmation);
            PreviousWorkflowState = WorkflowState.BillDemandInitiatorConfirmation;
        }

        #endregion

        #region Вспомогательные функции

        private void WriteTransitionToHistory(WorkflowState current, Guid initiatorId, Guid? impersonatedIdentityId)
        {
            WriteTransitionToHistory(current, initiatorId, impersonatedIdentityId, null);
        }


        private void WriteTransitionToHistory(WorkflowState current, Guid initiatorId, Guid? impersonatedIdentityId, DateTime? sightingTime)
        {
            if (DontWriteToWorkflowHistory)
                return;
            if (PreviousWorkflowState == null || PreviousWorkflowState.WorkflowStateName == WorkflowState.BillDemandDraft.WorkflowStateName)
            {
                Budget2WorkflowRuntime.BillDemandBuinessService.CreateBillDemandPreHistory(WorkflowInstanceId, WorkflowState.BillDemandDraft);
            }
            //Отказ не в черновик - расписываем маршрут
            else if (LastCommand.Id == WorkflowCommand.Denial.Id && current.WorkflowStateName != WorkflowState.BillDemandDraft.WorkflowStateName)
            {
                Budget2WorkflowRuntime.BillDemandBuinessService.CreateBillDemandPreHistory(WorkflowInstanceId, current);
            }

            if (PreviousWorkflowState == null)
            {
                Budget2WorkflowRuntime.BillDemandBuinessService.UpdateBillDemandState(current, WorkflowInstanceId);
                return;
            }

            if (sightingTime.HasValue)
                Budget2WorkflowRuntime.BillDemandBuinessService.UpdateBillDemandState(new UpdateBillDemandStateParams(PreviousWorkflowState, current, LastCommand,
                                                                                 WorkflowInstanceId,
                                                                                 initiatorId, impersonatedIdentityId, Comment, sightingTime.Value));
            else
                Budget2WorkflowRuntime.BillDemandBuinessService.UpdateBillDemandState(
                    new UpdateBillDemandStateParams(PreviousWorkflowState, current, LastCommand,
                                                    WorkflowInstanceId,
                                                    initiatorId, impersonatedIdentityId, Comment));
            Comment = string.Empty;
        }

        private void WriteTransitionToHistory(WorkflowState current)
        {
            WriteTransitionToHistory(current, null);
        }

        private void WriteTransitionToHistory(WorkflowState current, DateTime? sightingTime)
        {
            if (DontWriteToWorkflowHistory)
                return;
            SetStateIfParcelExists();
            WriteTransitionToHistory(current, TransitionInitiator, ImpersonatedIdentityId, sightingTime);
        }

        #endregion

        private void PaidInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandPaid);
            PreviousWorkflowState = WorkflowState.BillDemandPaid;
        }

        private void ExportBillDemandCode_ExecuteCode(object sender, EventArgs e)
        {
            try
            {
                Budget2WorkflowRuntime.BillDemandExportService.ExportBillDemand(
                Budget2WorkflowRuntime.BillDemandBuinessService.GetBillDemandForExport(WorkflowInstanceId));
            }
            catch (InvalidOperationException ex)
            {
                Budget2WorkflowRuntime.WorkflowParcelService.AddErrorMessageParcel(WorkflowInstanceId, ex.Message);
                throw;
            }

        }

        //private void CheckBillDemandIsCash_ExecuteCode(object sender, ConditionalEventArgs e)
        //{
        //    var paymentKind = Budget2WorkflowRuntime.BillDemandBuinessService.GetBillDemandPaymentKind(WorkflowInstanceId);
        //    e.Result = paymentKind.Id == PaymentKind.Cash.Id;
        //}

        private void CheckBillDemandFilialIsSupportedExport_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            // e.Result = Budget2WorkflowRuntime.BillDemandBuinessService.IsBillDemandSupportExport(WorkflowInstanceId);
            e.Result = !Budget2WorkflowRuntime.BillDemandBuinessService.IsBillDemandSkipInAccountingState(WorkflowInstanceId);
        }

        private void CheckBillDemandInitiatorHeadIsAutoSight_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            e.Result = Budget2WorkflowRuntime.BillDemandBuinessService.CheckInitiatorIsHead(WorkflowInstanceId);
            if (e.Result)
                TransitionInitiator =
                    Budget2WorkflowRuntime.BillDemandBuinessService.GetBillDemandInitiatorId(WorkflowInstanceId);
        }

        private void PrepareForRoute_ExecuteCode(object sender, EventArgs e)
        {
            Budget2WorkflowRuntime.BillDemandBuinessService.SetAllocationDate(WorkflowInstanceId);
            Budget2WorkflowRuntime.BillDemandBuinessService.DeleteDemandPermissions(WorkflowInstanceId);
            CheckBillDemand(null);
        }

        private void CheckBillDemand(WorkflowState state)
        {
            try
            {
                if (state == null)
                    Budget2WorkflowRuntime.BillDemandNotificationService.CheckAndSendMail(WorkflowInstanceId);
                else
                    Budget2WorkflowRuntime.BillDemandNotificationService.CheckAndSendMailForState(WorkflowInstanceId, state);
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat("Произошла ошибка при проверке РД. Message = {0}. BilDemandId = {1}", ex.Message, WorkflowInstanceId);
            }
        }

        private void SendNotifications(WorkflowState state)
        {
            try
            {
                if (state == null)
                    return;
                else
                    Budget2WorkflowRuntime.BillDemandNotificationService.SendNotificationsForState(WorkflowInstanceId, state);
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat("Произошла ошибка при отправке уведомления по РД. Message = {0}. BilDemandId = {1}", ex.Message, WorkflowInstanceId);
            }
        }

        [NonSerialized]
        private BillDemandExternalState _externalState = null;

        private void SetExternalParameters_ExecuteCode(object sender, EventArgs e)
        {
            Budget2WorkflowRuntime.BillDemandBuinessService.SetExternalParameters(WorkflowInstanceId, _externalState);
        }

        private void DraftForTechnicalDenialInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.BillDemandDraftForTechnicalDenial);
            Comment = string.Empty;
            StateToTransitFromDraft = PreviousWorkflowState;
            PreviousWorkflowState = WorkflowState.BillDemandDraftForTechnicalDenial;
        }

        private void SetBillDemandDatesCode_ExecuteCode(object sender, EventArgs e)
        {
            Budget2WorkflowRuntime.BillDemandBuinessService.SetTransferDateAndDateOfPerformance(WorkflowInstanceId);
        }

        private void CheckExternalStatus_ExecuteCode(object sender, EventArgs e)
        {
            _externalState = Budget2WorkflowRuntime.BillDemandExportService.GetBillDemandExternalStaus(WorkflowInstanceId);
        }

        private void CheckIsRejected_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            if (_externalState == null)
            {
                e.Result = false;
                return;
            }

            var codes = new[]
                            {
                                BillDemandExternalStatus.Rejected, BillDemandExternalStatus.RejectedInBOSS,
                                BillDemandExternalStatus.Destroyed
                            };

            e.Result = codes.Count(p => p.Code == _externalState.Status.Code) > 0;
            if (e.Result)
                Comment = GetExernalStatusComment();
        }

        private void CheckIsPaid_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            if (_externalState == null)
            {
                e.Result = false;
                return;
            }

            e.Result = _externalState.Status.Code == BillDemandExternalStatus.Paid.Code;

            if (e.Result)
                Comment = GetExernalStatusComment();

        }


        private void CheckIsOperative_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            IsOperative = e.Result = Budget2WorkflowRuntime.BillDemandBuinessService.IsOperative(WorkflowInstanceId);
        }

        private void WriteCommentToHistoryCode_ExecuteCode(object sender, EventArgs e)
        {
            if ((_externalState.Status.Code != LastExternalStatus.Code) && (!_externalState.Status.IsIgnored))
            {
                Comment = GetExernalStatusComment();
                WriteTransitionToHistory(WorkflowState.BillDemandOnPayment);
                LastExternalStatus = _externalState.Status;
            }
        }

        private string GetExernalStatusComment()
        {
            string comment = string.Empty;
            comment = string.Format("Статус во внешней системе {0}. Код = {1}.", _externalState.Status.Name, _externalState.Status.Code);
            if (_externalState.Status.RequiresDescription)
                comment += string.Format(" Причина {0}.", _externalState.Description);
            return comment;
        }

        private void exportEventInvoked(object sender, ExternalDataEventArgs e)
        {
            var workflowEventArgs = e as WorkflowEventArgsWithInitiator;
            if (workflowEventArgs == null)
                return;

            TransitionInitiator = workflowEventArgs.InitiatorId;
            ImpersonatedIdentityId = workflowEventArgs.ImpersonatedIdentityId;
            LastCommand = WorkflowCommand.Export;
        }

        private void paidEventInvoked(object sender, ExternalDataEventArgs e)
        {
            var workflowEventArgs = e as WorkflowEventArgsWithInitiator;
            if (workflowEventArgs == null)
                return;

            TransitionInitiator = workflowEventArgs.InitiatorId;
            ImpersonatedIdentityId = workflowEventArgs.ImpersonatedIdentityId;
            LastCommand = WorkflowCommand.SetPaid;

            var paidCommandEventArgs = e as PaidCommandEventArgs;
            if (paidCommandEventArgs == null)
                return;

            var externalState = new BillDemandExternalState()
                                    {
                                        DocumentNumber = paidCommandEventArgs.DocumentNumber,
                                        PaymentDate =
                                            paidCommandEventArgs.PaymentDate.HasValue
                                                ? paidCommandEventArgs.PaymentDate.Value
                                                : DateTime.Now
                                    };

            Budget2WorkflowRuntime.BillDemandBuinessService.SetExternalParameters(WorkflowInstanceId, externalState);


        }

        private void SetTransferDateCode_ExecuteCode(object sender, EventArgs e)
        {
            Budget2WorkflowRuntime.BillDemandBuinessService.SetTransferDate(WorkflowInstanceId);
        }

        private void StartProcessingEventInvoked(object sender, ExternalDataEventArgs e)
        {
            forvadTransitionEventInvoked(sender, e);
            LastCommand = WorkflowCommand.StartProcessing;
        }

    }
}