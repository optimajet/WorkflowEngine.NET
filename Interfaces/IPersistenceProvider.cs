using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Runtime.Timers;

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
        /// <exception cref="ProcessAlreadyExistsException"></exception>
        Task InitializeProcessAsync(ProcessInstance processInstance);

        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/>  and persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        Task FillProcessParametersAsync(ProcessInstance processInstance);

        /// <summary>
        /// Fills persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        Task FillPersistedProcessParametersAsync(ProcessInstance processInstance);

        /// <summary>
        /// Fills persisted <see cref="ParameterPurpose.Persistence"/> parameter of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        Task FillPersistedProcessParameterAsync(ProcessInstance processInstance, string parameterName);

        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        Task FillSystemProcessParametersAsync(ProcessInstance processInstance);

        /// <summary>
        /// Saves persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process to store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        Task SavePersistenceParametersAsync(ProcessInstance processInstance);

        /// <summary>
        /// Save persisted <see cref="ParameterPurpose.Persistence"/> parameter of the process to store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <param name="parameterName">Name of parameter for save</param>
        Task SavePersistenceParameterAsync(ProcessInstance processInstance, string parameterName);

        /// <summary>
        /// Remove persisted <see cref="ParameterPurpose.Persistence"/> parameter of the process from store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <param name="parameterName">Name of parameter for save</param>
        Task RemoveParameterAsync(ProcessInstance processInstance, string parameterName);

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Initialized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        Task SetWorkflowInitializedAsync(ProcessInstance processInstance);

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Idled"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        Task SetWorkflowIdledAsync(ProcessInstance processInstance);

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Running"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        Task SetWorkflowRunningAsync(ProcessInstance processInstance);

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Finalized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        Task SetWorkflowFinalizedAsync(ProcessInstance processInstance);

        /// <summary>
        /// Set process instance status to newStatus
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="newStatus">New process status</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        Task SetProcessStatusAsync(Guid processId, ProcessStatus newStatus);

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Terminated"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        Task SetWorkflowTerminatedAsync(ProcessInstance processInstance);

        /// <summary>
        /// Updates system parameters of the process in the store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <param name="transition">Last executed transition</param>
        Task UpdatePersistenceStateAsync(ProcessInstance processInstance, TransitionDefinition transition);

        /// <summary>
        /// Checks existence of the process
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
        Task<bool> IsProcessExistsAsync(Guid processId);

        /// <summary>
        /// Returns status of the process <see cref="ProcessStatus"/>
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns>Status of the process</returns>
        Task<ProcessStatus> GetInstanceStatusAsync(Guid processId);

        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process whith changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
        Task BindProcessToNewSchemeAsync(ProcessInstance processInstance);

        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process whith changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
        /// <param name="resetIsDeterminingParametersChanged">True if required to reset IsDeterminingParametersChanged flag <see cref="ProcessInstance.IsDeterminingParametersChanged"/></param>
        /// <exception cref="ProcessNotFoundException"></exception>
        Task BindProcessToNewSchemeAsync(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged);

        /// <summary>
        /// Register a new timer
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <param name="rootProcessId">Id of the root process</param>
        /// <param name="name">Timer name <see cref="TimerDefinition.Name"/></param>
        /// <param name="nextExecutionDateTime">Next date and time of timer's execution</param>
        /// <param name="notOverrideIfExists">If true specifies that the existing timer with same name will not be overriden <see cref="TimerDefinition.NotOverrideIfExists"/></param>
        Task RegisterTimerAsync(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists);

        /// <summary>
        /// Removes all timers from the store, exlude listed in ignore list
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <param name="timersIgnoreList">Ignore list</param>
        Task ClearTimersAsync(Guid processId, List<string> timersIgnoreList);

        /// <summary>
        /// Get all timers of a process
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
        Task<List<ProcessTimer>> GetTimersForProcessAsync(Guid processId);

        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processId">Id of the process</param>
        Task DeleteProcessAsync(Guid processId);

        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processIds">List of ids of the process</param>
        Task DeleteProcessAsync(Guid[] processIds);

        /// <summary>
        /// Saves a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        Task SaveGlobalParameterAsync<T>(string type, string name, T value);

        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Value of the parameter</returns>
        Task<T> LoadGlobalParameterAsync<T>(string type, string name);

        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <returns>List of the values of the parameters</returns>
        Task<List<T>> LoadGlobalParametersAsync<T>(string type);

        /// <summary>
        /// Deletes a global parameter
        /// </summary>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        Task DeleteGlobalParametersAsync(string type, string name = null);

        /// <summary>
        /// Returns the history of process
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
        Task<List<ProcessHistoryItem>> GetProcessHistoryAsync(Guid processId);

        bool IsBulkOperationsSupported { get; }

        Task BulkInitProcessesAsync(List<ProcessInstance> instances, ProcessStatus status, CancellationToken token);

        Task BulkInitProcessesAsync(List<ProcessInstance> instances, List<TimerToRegister> timers, ProcessStatus status, CancellationToken token);

        Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId);

        Task<bool> MultiServerRuntimesExistAsync();

        Task<WorkflowRuntimeModel> CreateWorkflowRuntimeAsync(string runtimeId, RuntimeStatus status);

        Task<WorkflowRuntimeModel> UpdateWorkflowRuntimeStatusAsync(WorkflowRuntimeModel runtime, RuntimeStatus status);

        Task<(bool Success, WorkflowRuntimeModel UpdatedModel)> UpdateWorkflowRuntimeRestorerAsync(WorkflowRuntimeModel runtime, string restorerId);

        Task<List<Guid>> GetRunningProcessesAsync(string runtimeId = null);

        Task<List<ProcessTimer>> GetActiveTimersForProcessAsync(Guid processId);

        Task DeleteInactiveTimersByProcessIdAsync(Guid processId);

        Task DeleteTimerAsync(Guid timerId);

        Task<List<Model.WorkflowTimer>> GetTopTimersToExecuteAsync(int top);

        Task<int> SetTimerIgnoreAsync(Guid id);

        Task<int> ActiveMultiServerRuntimesCountAsync(string currentRuntimeId);

        Task<WorkflowRuntimeModel> GetWorkflowRuntimeModelAsync(string runtimeId);

        Task<List<WorkflowRuntimeModel>> GetWorkflowRuntimesAsync();

        Task<int> SendRuntimeLastAliveSignalAsync();

        Task<DateTime?> GetNextTimerDateAsync(TimerCategory timerCategory, int timerInterval);

        Task DeleteWorkflowRuntimeAsync(string name);

        IApprovalProvider GetIApprovalProvider();

    }
}
