using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Persistence
{
    public interface IPersistenceProvider
    {
        void Init(WorkflowRuntime runtime);
        void InitializeProcess (ProcessInstance processInstance);
        void FillProcessParameters(ProcessInstance processInstance);
        void FillPersistedProcessParameters(ProcessInstance processInstance);
        void FillSystemProcessParameters(ProcessInstance processInstance);
        void SavePersistenceParameters(ProcessInstance processInstance);
        void SetWorkflowIniialized(ProcessInstance processInstance);
        void SetWorkflowIdled(ProcessInstance processInstance);
        void SetWorkflowRunning(ProcessInstance processInstance);
        void SetWorkflowFinalized(ProcessInstance processInstance);
        void SetWorkflowTerminated(ProcessInstance processInstance, ErrorLevel level, string errorMessage);
        void ResetWorkflowRunning();
        void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition);
        bool IsProcessExists(Guid processId);
        ProcessStatus GetInstanceStatus(Guid processId);
        void BindProcessToNewScheme(ProcessInstance processInstance);
        void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged);
        void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists);
        void ClearTimers(Guid processId, List<string> timersIgnoreList);
        void ClearTimersIgnore();
        void ClearTimer(Guid timerId);
        DateTime? GetCloseExecutionDateTime();
        List<TimerToExecute> GetTimersToExecute();
        void DeleteProcess(Guid processId);
        void DeleteProcess(Guid[] processIds);
    }
}
