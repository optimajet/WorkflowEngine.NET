using System;
using Budget2.Server.Workflow.Interface.DataContracts;

namespace Budget2.Server.Workflow.Interface.Services
{
    public interface IWorkflowParcelService
    {
        void AddParcel(Guid id, WorkflowSetStateParcel value);
        WorkflowSetStateParcel GetAndRemoveParcel(Guid id);
        void AddErrorMessageParcel(Guid id, string message);
        string GetAndRemoveMessage(Guid id);
    }
}
