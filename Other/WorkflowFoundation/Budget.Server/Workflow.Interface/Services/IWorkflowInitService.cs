using System;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Workflow.Interface.DataContracts;

namespace Budget2.Server.Workflow.Interface.Services
{
    [ExternalDataExchange]
    public interface IWorkflowInitService
    {
        WorkflowRuntime Runtime { get; }

        

        void SetWorkflowState(Guid instanceId, string stateName, string comment);

        /// <summary>
        /// Отправка на маршрут
        /// </summary>
        event EventHandler<WorkflowEventArgsWithInitiator> StartProcessing;

        /// <summary>
        /// Визирование
        /// </summary>
        event EventHandler<WorkflowEventArgsWithInitiator> Sighting;


        /// <summary>
        /// Отказ
        /// </summary>
        event EventHandler<DenialCommandEventArgs> Denial;

        /// <summary>
        /// Отказ по техническим причинам
        /// </summary>
        event EventHandler<DenialCommandEventArgs> DenialByTechnicalCauses;

        void RaiseStartProcessing(Guid instanceId);

        void RaiseSighting(Guid instanceId);
        void RaiseSighting(Guid instanceId, ServiceIdentity serviceIdentity);

        void RaiseDenial(Guid instanceId, string comment);

        void RaiseDenialByTechnicalCauses(Guid instanceId, string comment);

        
        void RaisePostingAccounting(Guid instanceId);

        void RaiseCheckStatus(Guid instanceId);
        void RaiseSetPaidStatus(Guid instanceId, DateTime? paymentDate, string documentNumber);
        void RaiseSetDenialStatus(Guid instanceId, string comment);
        void RaiseExport(Guid instanceId);
        void RaiseExport(Guid instanceId, ServiceIdentity serviceIdentity);
        void CreateWorkflowIfNotExists(Guid instanceId);
    }
}