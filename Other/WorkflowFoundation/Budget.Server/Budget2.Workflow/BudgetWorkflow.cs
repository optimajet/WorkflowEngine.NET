using System;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using Budget2.DAL.DataContracts;
using Budget2.Server.Workflow.Interface.DataContracts;
using Common.WF;

namespace Budget2.Workflow
{
    public class BudgetWorkflow : StateMachineWithSimpleContainer
    {

       
        #region Переменные состояния

        protected const string CommentPersistanceKey = "{4C9F905F-F9C4-4A23-920E-FEDB8C11BD1D}";

        public string Comment
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(CommentPersistanceKey))
                    return WorkflowPersistanceParameters[CommentPersistanceKey] as string;
                return string.Empty;
            }
            set
            {
                WorkflowPersistanceParameters[CommentPersistanceKey] = value;
            }
        }

        protected const string IsTechnicalDenialPersistanceKey = "{A844D5D9-E710-4726-978E-EF7E0ED02500}";

        /// <summary>
        /// Удалить
        /// </summary>
        [Obsolete]
        protected bool IsTechnicalDenial
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(IsTechnicalDenialPersistanceKey))
                    return (bool)WorkflowPersistanceParameters[IsTechnicalDenialPersistanceKey];
                return false;
            }
            set { WorkflowPersistanceParameters[IsTechnicalDenialPersistanceKey] = value; }
        }

        protected const string PreviousWorkflowStatePersistanceKey = "{6372FACB-EE0C-4DBF-8853-7C5913C778F1}";

        protected WorkflowState PreviousWorkflowState
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(PreviousWorkflowStatePersistanceKey))
                    return WorkflowPersistanceParameters[PreviousWorkflowStatePersistanceKey] as WorkflowState;
                return null;
            }
            set { WorkflowPersistanceParameters[PreviousWorkflowStatePersistanceKey] = value; }
        }

        protected const string StateToTransitFromDraftPersistanceKey = "{1A39C2CA-43BB-427D-A4AA-4DF1DEC9A286}";

        protected WorkflowState StateToTransitFromDraft
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(StateToTransitFromDraftPersistanceKey))
                    return WorkflowPersistanceParameters[StateToTransitFromDraftPersistanceKey] as WorkflowState;
                return null;
            }
            set { WorkflowPersistanceParameters[StateToTransitFromDraftPersistanceKey] = value; }
        }

        protected const string TransitionInitiatorPersistanceKey = "{4DF2EA14-CBB4-47FD-A8D5-593869A05131}";

        protected Guid TransitionInitiator
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(TransitionInitiatorPersistanceKey))
                    return (Guid)WorkflowPersistanceParameters[TransitionInitiatorPersistanceKey];
                return Guid.Empty;
            }
            set { WorkflowPersistanceParameters[TransitionInitiatorPersistanceKey] = value; }
        }

        protected const string ImpersonatedIdentityIdPersistanceKey = "{0B119185-1985-4191-A094-B43F32CB130E}";

        protected Guid? ImpersonatedIdentityId
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(ImpersonatedIdentityIdPersistanceKey))
                    return (Guid?)WorkflowPersistanceParameters[ImpersonatedIdentityIdPersistanceKey];
                return null;
            }
            set { WorkflowPersistanceParameters[ImpersonatedIdentityIdPersistanceKey] = value; }
        }

        protected const string LastCommandPersistanceKey = "{E7F11F46-6672-4119-A891-C6A8E1E55FE5}";

        protected WorkflowCommand LastCommand
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(LastCommandPersistanceKey))
                    return WorkflowPersistanceParameters[LastCommandPersistanceKey] as WorkflowCommand;
                return WorkflowCommand.Unknown;
            }
            set { WorkflowPersistanceParameters[LastCommandPersistanceKey] = value; }
        }

        #endregion

        #region Стандартные обработчики
        protected void setInternalParametersInvoked(object sender, ExternalDataEventArgs e)
        {
            var workflowEventArgs = e as SetWorkflowInternalParametersEventArgs;
            if (workflowEventArgs == null)
                return;

            foreach (var parameter in workflowEventArgs.Parameters)
            {
                if (WorkflowPersistanceParameters.ContainsKey(parameter.Key))
                    WorkflowPersistanceParameters[parameter.Key] = parameter.Value;
                else
                {
                    WorkflowPersistanceParameters.Add(parameter.Key,parameter.Value);
                }
            }
        }

        /// <summary>
        /// Стандартный обработчик прямого движения документа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void forvadTransitionEventInvoked(object sender, ExternalDataEventArgs e)
        {
            var workflowEventArgs = e as WorkflowEventArgsWithInitiator;
            if (workflowEventArgs == null)
                return;

            TransitionInitiator = workflowEventArgs.InitiatorId;
            ImpersonatedIdentityId = workflowEventArgs.ImpersonatedIdentityId;
            LastCommand = WorkflowCommand.Sighting;
        }

        /// <summary>
        /// Стандартный обработчик отказа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void denialEventFired1_Invoked(object sender, ExternalDataEventArgs e)
        {
            DenialEventFired(e);
            LastCommand = WorkflowCommand.Denial;
        }

        /// <summary>
        /// Стандартный обработчик отказа по ТП
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void technicalDenialEventFired1_Invoked(object sender, ExternalDataEventArgs e)
        {
            DenialEventFired(e);
            LastCommand = WorkflowCommand.DenialByTechnicalCauses;
        }

        protected void rollbackEventFired1_Invoked(object sender, ExternalDataEventArgs e)
        {
            DenialEventFired(e);
            LastCommand = WorkflowCommand.Rollback;
        }

        private void DenialEventFired(ExternalDataEventArgs e)
        {
            var workflowEventArgs = e as WorkflowEventArgsWithInitiator;
            if (workflowEventArgs == null)
                return;

            TransitionInitiator = workflowEventArgs.InitiatorId;
            ImpersonatedIdentityId = workflowEventArgs.ImpersonatedIdentityId;

            var args = e as DenialCommandEventArgs;
            if (args == null)
                return;

            Comment = args.Comment;
        }

        #endregion

        protected void SetStateIfParcelExists ()
        {
            var parcel = Budget2WorkflowRuntime.WorkflowParcelService.GetAndRemoveParcel(WorkflowInstanceId);
            if (parcel == null)
                return;

            if (!string.IsNullOrEmpty(parcel.Comment))
            {
                Comment = parcel.Comment;
            }

            if (parcel.PreviousWorkflowState != null)
                PreviousWorkflowState = parcel.PreviousWorkflowState;

            TransitionInitiator = parcel.InitiatorId;
            LastCommand = parcel.Command;
        }

    }
}
