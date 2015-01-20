using System;
using System.Workflow.Activities;
using Budget2.DAL.DataContracts;
using Common;

namespace Budget2.Workflow
{
    public sealed partial class Demand : BudgetWorkflow
    {
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

        private void WriteTransitionToHistory(WorkflowState current)
        {
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

    }
}
