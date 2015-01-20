using System;
using System.Collections.Generic;
using System.Workflow.Activities;
using Budget2.DAL.DataContracts;
using Common;

namespace Budget2.Workflow
{
    public sealed partial class Demand : BudgetWorkflow
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

        protected const string StateToTransitFromRollbackRequestedPersistanceKey = "{16096804-C28F-4B81-882F-156577441B9C}";

        protected WorkflowState StateToTransitFromRollbackRequested
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(StateToTransitFromRollbackRequestedPersistanceKey))
                    return WorkflowPersistanceParameters[StateToTransitFromRollbackRequestedPersistanceKey] as WorkflowState;
                return null;
            }
            set { WorkflowPersistanceParameters[StateToTransitFromRollbackRequestedPersistanceKey] = value; }
        }

        public Demand()
        {
            InitializeComponent();
        }

        private void DraftInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandDraft);
            PreviousWorkflowState = WorkflowState.DemandDraft;
        }

        private void OPExpertSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandOPExpertSighting);
            PreviousWorkflowState = WorkflowState.DemandOPExpertSighting;
            SendNotifications(WorkflowState.DemandOPExpertSighting);
        }

        private void OPHeadSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandOPHeadSighting);
            PreviousWorkflowState = WorkflowState.DemandOPHeadSighting;
            SendNotifications(WorkflowState.DemandOPHeadSighting);
        }

        private void InitiatorHeadSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandInitiatorHeadSighting);
            PreviousWorkflowState = WorkflowState.DemandInitiatorHeadSighting;
            SendNotifications(WorkflowState.DemandInitiatorHeadSighting);
        }

        private void UPKZCuratorSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandUPKZCuratorSighting);
            PreviousWorkflowState = WorkflowState.DemandUPKZCuratorSighting;
            SendNotifications(WorkflowState.DemandUPKZCuratorSighting);
        }

        private void UPKZHeadSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandUPKZHeadSighting);
            PreviousWorkflowState = WorkflowState.DemandUPKZHeadSighting;
            SendNotifications(WorkflowState.DemandUPKZHeadSighting);
        }

        private void AgreedInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandAgreed);
            PreviousWorkflowState = WorkflowState.DemandAgreed;
        }

        public void CheckInitiatorIsExecutorStructDivision_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            e.Result =
                Budget2WorkflowRuntime.DemandBusinessService.CheckInitiatorIsExecutorStructDivision(
                    WorkflowInstanceId);
        }

        public void CheckInitiatorIsAgreementStructDivision_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            e.Result =
                Budget2WorkflowRuntime.DemandBusinessService.CheckInitiatorIsAgreementStructDivision(
                    WorkflowInstanceId);
        }

        public void CheckSendToAgreementStructDivision_ExecuteCode(object sender, ConditionalEventArgs e)
        {
            e.Result =
                Budget2WorkflowRuntime.DemandBusinessService.CheckSendToAgreementStructDivision(
                    WorkflowInstanceId);
        }

        private void WriteTransitionToHistory(WorkflowState current)
        {
            if (DontWriteToWorkflowHistory)
                return;

            SetStateIfParcelExists();

            if (PreviousWorkflowState == null || PreviousWorkflowState.WorkflowStateName == WorkflowState.DemandDraft.WorkflowStateName)
            {
                Budget2WorkflowRuntime.DemandBusinessService.CreateDemandPreHistory(WorkflowInstanceId, WorkflowState.DemandDraft);
            }

            if (PreviousWorkflowState == null)
            {
                Budget2WorkflowRuntime.DemandBusinessService.UpdateDemandState(current, WorkflowInstanceId);
                return;
            }


            Budget2WorkflowRuntime.DemandBusinessService.UpdateDemandState(PreviousWorkflowState, current, LastCommand,
                                                                                  WorkflowInstanceId,
                                                                                  TransitionInitiator, Comment);

            Comment = string.Empty;
        }

        private void SendNotifications(WorkflowState state)
        {
            try
            {
                if (state == null)
                    return;
                else
                    Budget2WorkflowRuntime.DemandNotificationService.SendNotificationsForState(WorkflowInstanceId, state);
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat("Произошла ошибка при отправке уведомления по Заявке. Message = {0}. DemandId = {1}", ex.Message, WorkflowInstanceId);
            }
        }

        private void StartProcessingEventInvoked(object sender, ExternalDataEventArgs e)
        {
            forvadTransitionEventInvoked(sender, e);
            LastCommand = WorkflowCommand.StartProcessing;
        }

        private void AgreementOPExpertSightingInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandAgreementOPExpertSighting);
            PreviousWorkflowState = WorkflowState.DemandAgreementOPExpertSighting;
            SendNotifications(WorkflowState.DemandAgreementOPExpertSighting);
        }

        //private void AgreementInitiatorHeadSightingInitCode_ExecuteCode(object sender, EventArgs e)
        //{
        //    WriteTransitionToHistory(WorkflowState.DemandAgreementInitiatorHeadSighting);
        //    PreviousWorkflowState = WorkflowState.DemandAgreementInitiatorHeadSighting;
        //    SendNotifications(WorkflowState.DemandAgreementInitiatorHeadSighting);
        //}

        //private void AgreementOPHeadSightingInitCode_ExecuteCode(object sender, EventArgs e)
        //{
        //    WriteTransitionToHistory(WorkflowState.DemandAgreementOPHeadSighting);
        //    PreviousWorkflowState = WorkflowState.DemandAgreementOPHeadSighting;
        //    SendNotifications(WorkflowState.DemandAgreementOPHeadSighting);
        //}

        private void RollbackRequestedInitCode_ExecuteCode(object sender, EventArgs e)
        {
            WriteTransitionToHistory(WorkflowState.DemandRollbackRequested);
            Comment = string.Empty;
            StateToTransitFromRollbackRequested = PreviousWorkflowState;
            PreviousWorkflowState = WorkflowState.DemandRollbackRequested;
            SendNotifications(WorkflowState.DemandRollbackRequested);
        }



    }
}
