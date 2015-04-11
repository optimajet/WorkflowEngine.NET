using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Persistence
{
    /// <summary>
    /// Interface of a persistence provider, which provide storing of process's instance specific parameters and global parameters
    /// </summary>
    public interface IPersistenceProvider
    {
        /// <summary>
        /// Init the provider
        /// </summary>
        /// <param name="runtime">Workflow runtime instance which owned the provider</param>
        void Init(WorkflowRuntime runtime);
        /// <summary>
        /// Initialize a process instance in persistence store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void InitializeProcess (ProcessInstance processInstance);
        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/>  and persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void FillProcessParameters(ProcessInstance processInstance);
        /// <summary>
        /// Fills persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void FillPersistedProcessParameters(ProcessInstance processInstance);
        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void FillSystemProcessParameters(ProcessInstance processInstance);
        /// <summary>
        /// Saves persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process to store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void SavePersistenceParameters(ProcessInstance processInstance);
        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Initialized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void SetWorkflowIniialized(ProcessInstance processInstance);
        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Idled"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void SetWorkflowIdled(ProcessInstance processInstance);
        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Running"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void SetWorkflowRunning(ProcessInstance processInstance);
        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Finalized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void SetWorkflowFinalized(ProcessInstance processInstance);
        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Terminated"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        void SetWorkflowTerminated(ProcessInstance processInstance, ErrorLevel level, string errorMessage);
        /// <summary>
        /// Resets all process to <see cref="ProcessStatus.Idled"/> status
        /// </summary>
        void ResetWorkflowRunning();
        /// <summary>
        /// Updates system parameters of the process in the store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <param name="transition">Last executed transition</param>
        void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition);
        /// <summary>
        /// Checks existence of the process
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
        bool IsProcessExists(Guid processId);
        /// <summary>
        /// Returns status of the process <see cref="ProcessStatus"/>
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns>Status of the process</returns>
        ProcessStatus GetInstanceStatus(Guid processId);
        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process whith changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
        void BindProcessToNewScheme(ProcessInstance processInstance);

        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process whith changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
        /// <param name="resetIsDeterminingParametersChanged">True if required to reset IsDeterminingParametersChanged flag <see cref="ProcessInstance.IsDeterminingParametersChanged"/></param>
        void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged);
        /// <summary>
        /// Register a new timer
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <param name="name">Timer name <see cref="TimerDefinition.Name"/></param>
        /// <param name="nextExecutionDateTime">Next date and time of timer's execution</param>
        /// <param name="notOverrideIfExists">If true specifies that the existing timer with same name will not be overriden <see cref="TimerDefinition.NotOverrideIfExists"/></param>
        void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists);
        /// <summary>
        /// Removes all timers from the store, exlude listed in ignore list
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <param name="timersIgnoreList">Ignore list</param>
        void ClearTimers(Guid processId, List<string> timersIgnoreList);
        /// <summary>
        /// Clears sign Ignore for all timers
        /// </summary>
        void ClearTimersIgnore();
        /// <summary>
        /// Remove specific timer
        /// </summary>
        /// <param name="timerId">Id of the timer</param>
        void ClearTimer(Guid timerId);
        /// <summary>
        /// Get closest execution date and time for all timers
        /// </summary>
        /// <returns></returns>
        DateTime? GetCloseExecutionDateTime();
        /// <summary>
        /// Get all timers which must be executed at this moment of time
        /// </summary>
        /// <returns>List of timers to execute</returns>
        List<TimerToExecute> GetTimersToExecute();
        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processId">Id of the process</param>
        void DeleteProcess(Guid processId);
        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processIds">List of ids of the process</param>
        void DeleteProcess(Guid[] processIds);
        /// <summary>
        /// Saves a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        void SaveGlobalParameter<T>(string type, string name, T value); 
        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Value of the parameter</returns>
        T LoadGlobalParameter<T>(string type, string name);
        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <returns>List of the values of the parameters</returns>
        List<T> LoadGlobalParameters<T>(string type);
        /// <summary>
        /// Deletes a global parameter
        /// </summary>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        void DeleteGlobalParameters(string type, string name = null);
    }
}
