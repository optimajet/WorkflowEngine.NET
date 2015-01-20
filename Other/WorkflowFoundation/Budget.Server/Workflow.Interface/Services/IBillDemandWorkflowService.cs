using System;
using System.Workflow.Activities;
using Budget2.Server.Workflow.Interface.DataContracts;

namespace Budget2.Server.Workflow.Interface.Services
{
    [ExternalDataExchange]
    public interface IBillDemandWorkflowService
    {

        /// <summary>
        /// Отправка в бухгалтерию
        /// </summary>
        event EventHandler<WorkflowEventArgsWithInitiator> PostingAccounting;

        /// <summary>
        /// Проверка статуса в БОСС
        /// </summary>
        event EventHandler<WorkflowEventArgsWithInitiator> CheckStatus;

        /// <summary>
        /// Установка статуса оплачено
        /// </summary>
        event EventHandler<PaidCommandEventArgs> SetPaidStatus;

        /// <summary>
        /// Установка статуса отклонено
        /// </summary>
        event EventHandler<WorkflowEventArgsWithInitiator> SetDenialStatus;

        /// <summary>
        /// Установка статуса отклонено
        /// </summary>
        event EventHandler<WorkflowEventArgsWithInitiator> Export;

        void RaisePostingAccounting(WorkflowEventArgsWithInitiator args);

        void RaiseCheckStatus(WorkflowEventArgsWithInitiator args);


        void RaiseSetPaidStatus(PaidCommandEventArgs args);


        void RaiseSetDenialStatus(DenialCommandEventArgs args);


        void RaiseExport(WorkflowEventArgsWithInitiator args);
    }
}
