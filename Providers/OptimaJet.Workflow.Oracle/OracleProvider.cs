using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Runtime.Timers;
using Oracle.ManagedDataAccess.Client;
using WorkflowSync = OptimaJet.Workflow.Oracle.Models.WorkflowSync;

namespace OptimaJet.Workflow.Oracle
{
    public class OracleProvider : IWorkflowProvider
    {
        public string ConnectionString { get; set; }
        private string Schema { get; set; }
        private WorkflowRuntime _runtime;
        private readonly bool _writeToHistory;
        private readonly bool _writeSubProcessToRoot;

        public virtual void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public OracleProvider(string connectionString, string schema = null, bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            DbObject.SchemaName = schema;
            ConnectionString = connectionString;
            Schema = schema;
            _writeToHistory = writeToHistory;
            _writeSubProcessToRoot = writeSubProcessToRoot;
        }

        #region IPersistenceProvider

       public virtual async Task DeleteInactiveTimersByProcessIdAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowProcessTimer.DeleteInactiveByProcessIdAsync(connection, processId).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task DeleteTimerAsync(Guid timerId)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowProcessTimer.DeleteAsync(connection, timerId).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task<List<Guid>> GetRunningProcessesAsync(string runtimeId = null)
        {
            using var connection = new OracleConnection(ConnectionString);
            return await WorkflowProcessInstanceStatus.GetProcessesByStatusAsync(connection, ProcessStatus.Running.Id, runtimeId).ConfigureAwait(false);
        }

       public virtual async Task<WorkflowRuntimeModel> CreateWorkflowRuntimeAsync(string runtimeId, RuntimeStatus status)
        {
            using var connection = new OracleConnection(ConnectionString);
            var runtime = new Models.WorkflowRuntime() {RuntimeId = runtimeId, LOCKFLAG = Guid.NewGuid(), Status = status};

            await runtime.InsertAsync(connection).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);

            return new WorkflowRuntimeModel {Lock = runtime.LOCKFLAG, RuntimeId = runtimeId, Status = status};
        }

       public virtual async Task DeleteWorkflowRuntimeAsync(string name)
        {
            using var connection = new OracleConnection(ConnectionString);
            await Models.WorkflowRuntime.DeleteAsync(connection, name).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public async Task<List<ProcessInstanceItem>> GetProcessInstancesAsync(List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
       {
           using var connection = new OracleConnection(ConnectionString);
           
           WorkflowProcessInstance[] processInstances = await WorkflowProcessInstance.SelectAllWithPagingAsync(connection, orderParameters, paging).ConfigureAwait(false);

           return processInstances.Select(pi => new ProcessInstanceItem()
           {
               ActivityName  = pi.ActivityName,
               Id  = pi.Id,
               IsDeterminingParametersChanged  = pi.IsDeterminingParametersChanged,
               PreviousActivity  = pi.PreviousActivity,
               PreviousActivityForDirect  = pi.PreviousActivityForDirect,
               PreviousActivityForReverse  = pi.PreviousActivityForReverse,
               PreviousState  = pi.PreviousState,
               PreviousStateForDirect  = pi.PreviousStateForDirect,
               PreviousStateForReverse  = pi.PreviousStateForReverse,
               SchemeId  = pi.SchemeId,
               StateName  = pi.StateName,
               ParentProcessId  = pi.ParentProcessId,
               RootProcessId  = pi.RootProcessId,
               TenantId  = pi.TenantId,
               StartingTransition  = pi.StartingTransition,
               SubprocessName  = pi.SubprocessName,
               CreationDate  = pi.CreationDate,
               LastTransitionDate  = pi.LastTransitionDate,
           }).ToList();
       }

       public async Task<int> GetProcessInstancesCountAsync()
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowProcessInstance.GetCountAsync(connection).ConfigureAwait(false);
       }

       public async Task<List<SchemeItem>> GetSchemesAsync(List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
       {
           using var connection = new OracleConnection(ConnectionString);
           
           WorkflowScheme[] schemes = await WorkflowScheme.SelectAllWithPagingAsync(connection, orderParameters, paging).ConfigureAwait(false);

           return schemes.Select(sc => new SchemeItem()
           {
               Code = sc.Code,
               Scheme = sc.Scheme,
               CanBeInlined = sc.CanBeInlined,
               InlinedSchemes = sc.GetInlinedSchemes(),
               Tags = TagHelper.FromTagString(sc.Tags)
           }).ToList();
       }
       
       public async Task<int> GetSchemesCountAsync()
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowScheme.GetCountAsync(connection).ConfigureAwait(false);
       }

       public virtual async Task<WorkflowRuntimeModel> UpdateWorkflowRuntimeStatusAsync(WorkflowRuntimeModel runtime, RuntimeStatus status)
        {
            Tuple<int, WorkflowRuntimeModel> res = await UpdateWorkflowRuntimeAsync(runtime, x => x.Status = status, Models.WorkflowRuntime.UpdateStatusAsync).ConfigureAwait(false);

            if (res.Item1 != 1)
            {
                throw new ImpossibleToSetRuntimeStatusException();
            }

            return res.Item2;
        }

       public virtual async Task<(bool Success, WorkflowRuntimeModel UpdatedModel)> UpdateWorkflowRuntimeRestorerAsync(WorkflowRuntimeModel runtime, string restorerId)
        {
            Tuple<int, WorkflowRuntimeModel> res = await UpdateWorkflowRuntimeAsync(runtime, x => x.RestorerId = restorerId, Models.WorkflowRuntime.UpdateRestorerAsync).ConfigureAwait(false);

            return (res.Item1 == 1, res.Item2);
        }

       public virtual async Task<bool> MultiServerRuntimesExistAsync()
        {
            using var connection = new OracleConnection(ConnectionString);
            return await Models.WorkflowRuntime.MultiServerRuntimesExistAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task<int> ActiveMultiServerRuntimesCountAsync(string currentRuntimeId)
        {
            using var connection = new OracleConnection(ConnectionString);
            return await Models.WorkflowRuntime.ActiveMultiServerRuntimesCountAsync(connection, currentRuntimeId).ConfigureAwait(false);
        }

       public virtual async Task InitializeProcessAsync(ProcessInstance processInstance)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstance oldProcess = await WorkflowProcessInstance.SelectByKeyAsync(connection, processInstance.ProcessId).ConfigureAwait(false);
            if (oldProcess != null)
            {
                throw new ProcessAlreadyExistsException(processInstance.ProcessId);
            }

            var newProcess = new WorkflowProcessInstance
            {
                Id = processInstance.ProcessId,
                SchemeId = processInstance.SchemeId,
                ActivityName = processInstance.ProcessScheme.InitialActivity.Name,
                StateName = processInstance.ProcessScheme.InitialActivity.State,
                RootProcessId = processInstance.RootProcessId,
                ParentProcessId = processInstance.ParentProcessId,
                TenantId = processInstance.TenantId,
                StartingTransition = processInstance.ProcessScheme.StartingTransition,
                SubprocessName = processInstance.SubprocessName,
                CreationDate = processInstance.CreationDate
            };
            await newProcess.InsertAsync(connection).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance)
        {
            await BindProcessToNewSchemeAsync(processInstance, false).ConfigureAwait(false);
        }

       public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstance oldProcess = await WorkflowProcessInstance.SelectByKeyAsync(connection, processInstance.ProcessId).ConfigureAwait(false);
            if (oldProcess == null)
            {
                throw new ProcessNotFoundException(processInstance.ProcessId);
            }

            oldProcess.SchemeId = processInstance.SchemeId;
            oldProcess.StartingTransition = processInstance.ProcessScheme.StartingTransition;
            if (resetIsDeterminingParametersChanged)
            {
                oldProcess.IsDeterminingParametersChanged = false;
            }

            await oldProcess.UpdateAsync(connection).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task FillProcessParametersAsync(ProcessInstance processInstance)
        {
            processInstance.AddParameters(await GetProcessParametersAsync(processInstance.ProcessId, processInstance.ProcessScheme).ConfigureAwait(false));
        }

       public virtual async Task FillPersistedProcessParametersAsync(ProcessInstance processInstance)
        {
            processInstance.AddParameters(await GetPersistedProcessParametersAsync(processInstance.ProcessId, processInstance.ProcessScheme).ConfigureAwait(false));
        }

       public virtual async Task FillPersistedProcessParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            ParameterDefinitionWithValue persistedProcessParameter = await GetPersistedProcessParameterAsync(processInstance.ProcessId, processInstance.ProcessScheme, parameterName).ConfigureAwait(false);
            if (persistedProcessParameter == null)
            {
                return;
            }
            processInstance.AddParameter(persistedProcessParameter);
        }

       public virtual async Task FillSystemProcessParametersAsync(ProcessInstance processInstance)
        {
            processInstance.AddParameters(await GetSystemProcessParametersAsync(processInstance.ProcessId, processInstance.ProcessScheme).ConfigureAwait(false));
        }

       public virtual async Task SavePersistenceParametersAsync(ProcessInstance processInstance)
        {
            var parametersToPersistList = processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence)
                                                                           .Select(ptp => ParameterDefinitionWithValueToDynamic(ptp)).ToList();

            using var connection = new OracleConnection(ConnectionString);
            var persistedParameters = (await WorkflowProcessInstancePersistence.SelectByProcessIdAsync(connection, processInstance.ProcessId).ConfigureAwait(false)).ToList();

            foreach (dynamic parameterDefinitionWithValue in parametersToPersistList)
            {
                WorkflowProcessInstancePersistence persistence =
                    persistedParameters.SingleOrDefault(
                        pp => pp.ParameterName == parameterDefinitionWithValue.Parameter.Name);

                await InsertOrUpdateParameterAsync(connection, processInstance, parameterDefinitionWithValue, persistence).ConfigureAwait(false);
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }
       public virtual async Task SavePersistenceParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            dynamic parameter = ParameterDefinitionWithValueToDynamic(processInstance.ProcessParameters.Single(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Name == parameterName));
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstancePersistence persistence = await WorkflowProcessInstancePersistence.SelectByNameAsync(connection, processInstance.ProcessId, parameterName).ConfigureAwait(false);
            await InsertOrUpdateParameterAsync(connection, processInstance, parameter, persistence).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

        private dynamic ParameterDefinitionWithValueToDynamic(ParameterDefinitionWithValue ptp)
        {
            string serializedValue = ptp.Type == typeof(UnknownParameterType) ? (string)ptp.Value : ParametersSerializer.Serialize(ptp.Value, ptp.Type);
            return new { Parameter = ptp, SerializedValue = serializedValue };
        }

        private async Task InsertOrUpdateParameterAsync(OracleConnection connection, ProcessInstance processInstance, dynamic parameter, WorkflowProcessInstancePersistence persistence)
        {
            if (persistence == null)
            {
                if (parameter.SerializedValue != null)
                {
                    persistence = new WorkflowProcessInstancePersistence()
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = processInstance.ProcessId,
                        ParameterName = parameter.Parameter.Name,
                        Value = parameter.SerializedValue
                    };
                    await persistence.InsertAsync(connection).ConfigureAwait(false);
                }
            }
            else
            {
                if (parameter.SerializedValue != null)
                {
                    persistence.Value = parameter.SerializedValue;
                    await persistence.UpdateAsync(connection).ConfigureAwait(false);
                }
                else
                {
                    await WorkflowProcessInstancePersistence.DeleteAsync(connection, persistence.Id).ConfigureAwait(false);
                }
            }
        }

       public virtual async Task RemoveParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowProcessInstancePersistence.DeleteByNameAsync(connection, processInstance.ProcessId, parameterName).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task SetProcessStatusAsync(Guid processId, ProcessStatus newStatus)
        {
            if (newStatus == ProcessStatus.Running)
            {
               await SetRunningStatusAsync(processId).ConfigureAwait(false);
            }
            else
            {
               await SetCustomStatusAsync(processId, newStatus).ConfigureAwait(false);
            }
        }

       public virtual async Task SetWorkflowInitializedAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Initialized, true).ConfigureAwait(false);
        }

       public virtual async Task SetWorkflowIdledAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Idled).ConfigureAwait(false);
        }

       public virtual async Task SetWorkflowRunningAsync(ProcessInstance processInstance)
        {
            Guid processId = processInstance.ProcessId;
            await SetRunningStatusAsync(processId).ConfigureAwait(false);
        }

       public virtual async Task SetWorkflowFinalizedAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Finalized).ConfigureAwait(false);
        }

       public virtual async Task SetWorkflowTerminatedAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Terminated).ConfigureAwait(false);
        }

       public async Task WriteInitialRecordToHistoryAsync(ProcessInstance processInstance)
       {
           if (!_writeToHistory) { return; }

           using var connection = new OracleConnection(ConnectionString);

           var history = new WorkflowProcessTransitionHistory
           {
               Id = Guid.NewGuid(),
               ProcessId = _writeSubProcessToRoot && processInstance.IsSubprocess
                   ? processInstance.RootProcessId
                   : processInstance.ProcessId,
               FromActivityName = String.Empty,
               FromStateName = String.Empty,
               ToActivityName = processInstance.CurrentActivityName,
               ToStateName = processInstance.CurrentState,
               TransitionClassifier = nameof(TransitionClassifier.NotSpecified),
               TransitionTime = _runtime.RuntimeDateTimeNow,
               TriggerName = "Initializing",
               StartTransitionTime = _runtime.RuntimeDateTimeNow,
               TransitionDuration = 0
           };

           await history.InsertAsync(connection).ConfigureAwait(false);
       }

       public virtual async Task UpdatePersistenceStateAsync(ProcessInstance processInstance, TransitionDefinition transition)
        {
            DateTime startTransitionTime = processInstance.StartTransitionTime ?? _runtime.RuntimeDateTimeNow;
            
            ParameterDefinitionWithValue paramIdentityId = await processInstance.GetParameterAsync(DefaultDefinitions.ParameterIdentityId.Name).ConfigureAwait(false);
            ParameterDefinitionWithValue paramImpIdentityId = await processInstance.GetParameterAsync(DefaultDefinitions.ParameterImpersonatedIdentityId.Name).ConfigureAwait(false);

            string identityId = paramIdentityId == null ? String.Empty : (string)paramIdentityId.Value;
            string impIdentityId = paramImpIdentityId == null ? identityId : (string)paramImpIdentityId.Value;

            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstance inst = await WorkflowProcessInstance.SelectByKeyAsync(connection, processInstance.ProcessId).ConfigureAwait(false);

            if (inst != null)
            {
                if (!String.IsNullOrEmpty(transition.To.State))
                {
                    inst.StateName = transition.To.State;
                }

                inst.ActivityName = transition.To.Name;
                inst.PreviousActivity = transition.From.Name;

                if (!String.IsNullOrEmpty(transition.From.State))
                {
                    inst.PreviousState = transition.From.State;
                }

                if (transition.Classifier == TransitionClassifier.Direct)
                {
                    inst.PreviousActivityForDirect = transition.From.Name;

                    if (!String.IsNullOrEmpty(transition.From.State))
                    {
                        inst.PreviousStateForDirect = transition.From.State;
                    }
                }
                else if (transition.Classifier == TransitionClassifier.Reverse)
                {
                    inst.PreviousActivityForReverse = transition.From.Name;
                    if (!String.IsNullOrEmpty(transition.From.State))
                    {
                        inst.PreviousStateForReverse = transition.From.State;
                    }
                }

                inst.ParentProcessId = processInstance.ParentProcessId;
                inst.RootProcessId = processInstance.RootProcessId;
                inst.LastTransitionDate = processInstance.LastTransitionDate;

                await inst.UpdateAsync(connection).ConfigureAwait(false);
            }

            if (_writeToHistory)
            {
                var history = new WorkflowProcessTransitionHistory()
                {
                    ActorIdentityId = impIdentityId,
                    ExecutorIdentityId = identityId,
                    Id = Guid.NewGuid(),
                    IsFinalised = transition.To.IsFinal,
                    ProcessId = _writeSubProcessToRoot && processInstance.IsSubprocess ? processInstance.RootProcessId : processInstance.ProcessId,
                    FromActivityName = transition.From.Name,
                    FromStateName = transition.From.State,
                    ToActivityName = transition.To.Name,
                    ToStateName = transition.To.State,
                    TransitionClassifier = transition.Classifier.ToString(),
                    TransitionTime = _runtime.RuntimeDateTimeNow,
                    TriggerName = String.IsNullOrEmpty(processInstance.ExecutedTimer) ? processInstance.CurrentCommand : processInstance.ExecutedTimer,
                    StartTransitionTime = startTransitionTime,
                    TransitionDuration = (int)(_runtime.RuntimeDateTimeNow - startTransitionTime).TotalMilliseconds
                };
                await history.InsertAsync(connection).ConfigureAwait(false);
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task<bool> IsProcessExistsAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            return await WorkflowProcessInstance.SelectByKeyAsync(connection, processId).ConfigureAwait(false) != null;
        }

       public virtual async Task<ProcessStatus> GetInstanceStatusAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstanceStatus instance = await WorkflowProcessInstanceStatus.SelectByKeyAsync(connection, processId).ConfigureAwait(false);
            if (instance == null)
            {
                return ProcessStatus.NotFound;
            }

            ProcessStatus status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status);
            if (status == null)
            {
                return ProcessStatus.Unknown;
            }

            return status;
        }

        private async Task SetRunningStatusAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstanceStatus instanceStatus = await WorkflowProcessInstanceStatus.SelectByKeyAsync(connection, processId).ConfigureAwait(false);
            if (instanceStatus == null)
            {
                throw new StatusNotDefinedException();
            }

            if (instanceStatus.Status == ProcessStatus.Running.Id)
            {
                throw new ImpossibleToSetStatusException("Process already running");
            }

            Guid oldLock = instanceStatus.LOCKFLAG;

            instanceStatus.LOCKFLAG = Guid.NewGuid();
            instanceStatus.Status = ProcessStatus.Running.Id;
            instanceStatus.RuntimeId = _runtime.Id;
            instanceStatus.SetTime = _runtime.RuntimeDateTimeNow;

            int cnt = await WorkflowProcessInstanceStatus.ChangeStatusAsync(connection, instanceStatus, oldLock).ConfigureAwait(false);
            
            if (cnt == 0)
            {
                instanceStatus = await WorkflowProcessInstanceStatus.SelectByKeyAsync(connection, processId).ConfigureAwait(false);
                if (instanceStatus == null)
                {
                    throw new StatusNotDefinedException();
                }
            }

            if (cnt != 1)
            {
                throw new ImpossibleToSetStatusException();
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

        private async Task SetCustomStatusAsync(Guid processId, ProcessStatus status, bool createIfNotDefined = false)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstanceStatus instanceStatus = await WorkflowProcessInstanceStatus.SelectByKeyAsync(connection, processId).ConfigureAwait(false);
            if (instanceStatus == null)
            {
                if (!createIfNotDefined)
                {
                    throw new StatusNotDefinedException();
                }

                instanceStatus = new WorkflowProcessInstanceStatus()
                {
                    Id = processId,
                    LOCKFLAG = Guid.NewGuid(),
                    Status = status.Id,
                    RuntimeId = _runtime.Id,
                    SetTime = _runtime.RuntimeDateTimeNow
                };
                
                await instanceStatus.InsertAsync(connection).ConfigureAwait(false);
            }
            else
            {
                Guid oldLock = instanceStatus.LOCKFLAG;

                instanceStatus.Status = status.Id;
                instanceStatus.LOCKFLAG = Guid.NewGuid();
                instanceStatus.RuntimeId = _runtime.Id;
                instanceStatus.SetTime = _runtime.RuntimeDateTimeNow;

                int cnt = await WorkflowProcessInstanceStatus.ChangeStatusAsync(connection, instanceStatus, oldLock).ConfigureAwait(false);
                
                if (cnt == 0)
                {
                    instanceStatus = await WorkflowProcessInstanceStatus.SelectByKeyAsync(connection, processId).ConfigureAwait(false);
                    if (instanceStatus == null)
                    {
                        throw new StatusNotDefinedException();
                    }
                }

                if (cnt != 1)
                {
                    throw new ImpossibleToSetStatusException();
                }
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

        private async Task<IEnumerable<ParameterDefinitionWithValue>> GetProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
        {
            var parameters = new List<ParameterDefinitionWithValue>(processDefinition.Parameters.Count);
            parameters.AddRange(await GetPersistedProcessParametersAsync(processId, processDefinition).ConfigureAwait(false));
            parameters.AddRange(await GetSystemProcessParametersAsync(processId, processDefinition).ConfigureAwait(false));
            return parameters;
        }

        private async Task<IEnumerable<ParameterDefinitionWithValue>> GetSystemProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
        {
            WorkflowProcessInstance processInstance = await GetProcessInstanceAsync(processId).ConfigureAwait(false);

            var systemParameters =
                processDefinition.Parameters.Where(p => p.Purpose == ParameterPurpose.System).ToList();

            var parameters = new List<ParameterDefinitionWithValue>(systemParameters.Count)
            {
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterProcessId.Name),
                    processId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousState.Name),
                    processInstance.PreviousState),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCurrentState.Name),
                    processInstance.StateName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousStateForDirect.Name),
                    processInstance.PreviousStateForDirect),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousStateForReverse.Name),
                    processInstance.PreviousStateForReverse),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivity.Name),
                    processInstance.PreviousActivity),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCurrentActivity.Name),
                    processInstance.ActivityName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivityForDirect.Name),
                    processInstance.PreviousActivityForDirect),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivityForReverse.Name),
                    processInstance.PreviousActivityForReverse),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSchemeCode.Name),
                    processDefinition.Name),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSchemeId.Name),
                    processInstance.SchemeId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterIsPreExecution.Name),
                    false),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterParentProcessId.Name),
                    processInstance.ParentProcessId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterRootProcessId.Name),
                    processInstance.RootProcessId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterTenantId.Name),
                    processInstance.TenantId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSubprocessName.Name),
                    processInstance.SubprocessName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCreationDate.Name),
                    processInstance.CreationDate),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterLastTransitionDate.Name),
                    processInstance.LastTransitionDate)
            };
            return parameters;
        }

        private async Task<IEnumerable<ParameterDefinitionWithValue>> GetPersistedProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count);

            List<WorkflowProcessInstancePersistence> persistedParameters;

            using (var connection = new OracleConnection(ConnectionString))
            {
                persistedParameters = (await WorkflowProcessInstancePersistence.SelectByProcessIdAsync(connection, processId).ConfigureAwait(false)).ToList();
            }

            foreach (WorkflowProcessInstancePersistence persistedParameter in persistedParameters)
            {
                parameters.Add(WorkflowProcessInstancePersistenceToParameterDefinitionWithValue(persistenceParameters, persistedParameter));
            }

            return parameters;
        }

        private async Task<ParameterDefinitionWithValue> GetPersistedProcessParameterAsync(Guid processId, ProcessDefinition processDefinition, string parameterName)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            WorkflowProcessInstancePersistence persistedParameter;
            using (var connection = new OracleConnection(ConnectionString))
            {
                persistedParameter = await WorkflowProcessInstancePersistence.SelectByNameAsync(connection, processId, parameterName).ConfigureAwait(false);
            }

            if (persistedParameter == null)
            {
                return null;
            }

            return WorkflowProcessInstancePersistenceToParameterDefinitionWithValue(persistenceParameters, persistedParameter);
        }

        private ParameterDefinitionWithValue WorkflowProcessInstancePersistenceToParameterDefinitionWithValue(List<ParameterDefinition> persistenceParameters, WorkflowProcessInstancePersistence persistedParameter)
        {
            ParameterDefinition parameterDefinition = persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.ParameterName);
            if (parameterDefinition == null)
            {
                parameterDefinition = ParameterDefinition.Create(persistedParameter.ParameterName, typeof(UnknownParameterType), ParameterPurpose.Persistence);
                return ParameterDefinition.Create(parameterDefinition, persistedParameter.Value);
            }

            return ParameterDefinition.Create(parameterDefinition, ParametersSerializer.Deserialize(persistedParameter.Value, parameterDefinition.Type));
        }


        private async Task<WorkflowProcessInstance> GetProcessInstanceAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstance processInstance = await WorkflowProcessInstance.SelectByKeyAsync(connection, processId).ConfigureAwait(false);
            if (processInstance == null)
            {
                throw new ProcessNotFoundException(processId);
            }

            return processInstance;
        }

       public virtual async Task DeleteProcessAsync(Guid[] processIds)
        {
            foreach (Guid processId in processIds)
            {
                await DeleteProcessAsync(processId).ConfigureAwait(false);
            }
        }

       public virtual async Task DeleteProcessAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowProcessInstance.DeleteAsync(connection, processId).ConfigureAwait(false);
            await WorkflowProcessInstanceStatus.DeleteAsync(connection, processId).ConfigureAwait(false) ;
            await WorkflowProcessInstancePersistence.DeleteByProcessIdAsync(connection, processId).ConfigureAwait(false);
            await WorkflowProcessTransitionHistory.DeleteByProcessIdAsync(connection, processId).ConfigureAwait(false);
            await WorkflowProcessTimer.DeleteByProcessIdAsync(connection, processId).ConfigureAwait(false);
            await WorkflowInbox.DeleteByProcessIdAsync(connection, processId).ConfigureAwait(false);
            await WorkflowApprovalHistory.DeleteByProcessIdAsync(connection, processId).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task RegisterTimerAsync(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessTimer timer = await WorkflowProcessTimer.SelectByProcessIdAndNameAsync(connection, processId, name).ConfigureAwait(false);
            if (timer == null)
            {
                timer = new WorkflowProcessTimer
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    NextExecutionDateTime = nextExecutionDateTime,
                    ProcessId = processId,
                    RootProcessId = rootProcessId,
                    Ignore = false
                };

                await timer.InsertAsync(connection).ConfigureAwait(false);
            }
            else if (!notOverrideIfExists)
            {
                timer.NextExecutionDateTime = nextExecutionDateTime;
                await timer.UpdateAsync(connection).ConfigureAwait(false);
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task ClearTimersAsync(Guid processId, List<string> timersIgnoreList)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowProcessTimer.DeleteByProcessIdAsync(connection, processId, timersIgnoreList).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task<int> SetTimerIgnoreAsync(Guid timerId)
        {
            using var connection = new OracleConnection(ConnectionString);
            int result = await WorkflowProcessTimer.SetTimerIgnoreAsync(connection, timerId).ConfigureAwait(false);
            if (result > 0)
            {
                await DbObject.CommitAsync(connection).ConfigureAwait(false);
            }

            return result;
        }

       public virtual async Task<List<Core.Model.WorkflowTimer>> GetTopTimersToExecuteAsync(int top)
        {
            DateTime now = _runtime.RuntimeDateTimeNow;

            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessTimer[] timers = await WorkflowProcessTimer.GetTopTimersToExecuteAsync(connection, top, now).ConfigureAwait(false);

            if (timers.Length == 0)
            {
                return new List<Core.Model.WorkflowTimer>();
            }

            return timers.Select(t => new Core.Model.WorkflowTimer()
            {
                Name = t.Name,
                ProcessId = t.ProcessId,
                TimerId = t.Id,
                NextExecutionDateTime = t.NextExecutionDateTime,
                RootProcessId = t.RootProcessId
            }).ToList();
        }

       public virtual async Task SaveGlobalParameterAsync<T>(string type, string name, T value)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowGlobalParameter parameter = (await WorkflowGlobalParameter.SelectByTypeAndNameAsync(connection, type, name).ConfigureAwait(false)).FirstOrDefault();

            if (parameter == null)
            {
                parameter = new WorkflowGlobalParameter() {Id = Guid.NewGuid(), Type = type, Name = name, Value = JsonConvert.SerializeObject(value)};

                await parameter.InsertAsync(connection).ConfigureAwait(false);
            }
            else
            {
                parameter.Value = JsonConvert.SerializeObject(value);

                await parameter.UpdateAsync(connection).ConfigureAwait(false);
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task<T> LoadGlobalParameterAsync<T>(string type, string name)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowGlobalParameter parameter = (await WorkflowGlobalParameter.SelectByTypeAndNameAsync(connection, type, name).ConfigureAwait(false)).FirstOrDefault();

            if (parameter == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(parameter.Value);
        }

       public virtual async Task<List<T>> LoadGlobalParametersAsync<T>(string type)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowGlobalParameter[] parameters = await WorkflowGlobalParameter.SelectByTypeAndNameAsync(connection, type).ConfigureAwait(false);

            return parameters.Select(p => JsonConvert.DeserializeObject<T>(p.Value)).ToList();
        }

       public virtual async Task DeleteGlobalParametersAsync(string type, string name = null)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowGlobalParameter.DeleteByTypeAndNameAsync(connection, type, name).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task<List<ProcessHistoryItem>> GetProcessHistoryAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            return (await WorkflowProcessTransitionHistory.SelectByProcessIdAsync(connection, processId).ConfigureAwait(false))
                .Select(hi => new ProcessHistoryItem
                {
                    ActorIdentityId = hi.ActorIdentityId,
                    ExecutorIdentityId = hi.ExecutorIdentityId,
                    FromActivityName = hi.FromActivityName,
                    FromStateName = hi.FromStateName,
                    IsFinalised = hi.IsFinalised,
                    ProcessId = hi.ProcessId,
                    ToActivityName = hi.ToActivityName,
                    ToStateName = hi.ToStateName,
                    TransitionClassifier = (TransitionClassifier)Enum.Parse(typeof(TransitionClassifier), hi.TransitionClassifier),
                    TransitionTime = hi.TransitionTime,
                    TriggerName = hi.TriggerName,
                    StartTransitionTime = hi.StartTransitionTime,
                    TransitionDuration = hi.TransitionDuration
                })
                .ToList();
        }

       public virtual async Task<List<ProcessTimer>> GetTimersForProcessAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            IEnumerable<WorkflowProcessTimer> timers = await WorkflowProcessTimer.SelectByProcessIdAsync(connection, processId).ConfigureAwait(false);
            return timers.Select(t => new ProcessTimer(t.Id, t.Name, t.NextExecutionDateTime)).ToList();
        }

       public virtual async Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId)
        {
            using var connection = new OracleConnection(ConnectionString);
            return await ProcessInstanceTreeItem.GetProcessTreeItemsByRootProcessId(connection, rootProcessId).ConfigureAwait(false);
        }

       public virtual async Task<List<ProcessTimer>> GetActiveTimersForProcessAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            IEnumerable<WorkflowProcessTimer> timers = await WorkflowProcessTimer.SelectActiveByProcessIdAsync(connection, processId).ConfigureAwait(false);
            return timers.Select(t => new ProcessTimer(t.Id, t.Name, t.NextExecutionDateTime)).ToList();
        }

       public virtual async Task<WorkflowRuntimeModel> GetWorkflowRuntimeModelAsync(string runtimeId)
        {
            using var connection = new OracleConnection(ConnectionString);
            return await Models.WorkflowRuntime.GetWorkflowRuntimeStatusAsync(connection, runtimeId).ConfigureAwait(false);
        }

       public virtual async Task<int> SendRuntimeLastAliveSignalAsync()
        {
            using var connection = new OracleConnection(ConnectionString);
            int result = await Models.WorkflowRuntime.SendRuntimeLastAliveSignalAsync(connection, _runtime.Id, _runtime.RuntimeDateTimeNow).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
            return result;
        }


       public virtual async Task<DateTime?> GetNextTimerDateAsync(TimerCategory timerCategory, int timerInterval)
        {
            using var connection = new OracleConnection(ConnectionString);
            string timerCategoryName = timerCategory.ToString();
            WorkflowSync syncLock = await WorkflowSync.GetByNameAsync(connection, timerCategoryName).ConfigureAwait(false);

            if (syncLock == null)
            {
                throw new Exception($"Sync lock {timerCategoryName} not found");
            }

            string nextTimeColumnName;

            switch (timerCategory)
            {
                case TimerCategory.Timer:
                    nextTimeColumnName = "NextTimerTime";
                    break;
                case TimerCategory.ServiceTimer:
                    nextTimeColumnName = "NextServiceTimerTime";
                    break;
                default:
                    throw new Exception($"Unknown sync lock name: {timerCategoryName}");
            }

            DateTime? max = await Models.WorkflowRuntime.GetMaxNextTimeAsync(connection, _runtime.Id, nextTimeColumnName).ConfigureAwait(false);

            DateTime result = _runtime.RuntimeDateTimeNow;

            if (max > result)
            {
                result = max.Value;
            }

            result += TimeSpan.FromMilliseconds(timerInterval);

            var newLock = Guid.NewGuid();

            await Models.WorkflowRuntime.UpdateNextTimeAsync(connection, _runtime.Id, nextTimeColumnName, result).ConfigureAwait(false);
            int rowCount = await WorkflowSync.UpdateLockAsync(connection, timerCategoryName, syncLock.LOCKFLAG, newLock).ConfigureAwait(false);

            if (rowCount == 0)
            {
                return null;
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);

            return result;
        }

       public virtual async Task<List<WorkflowRuntimeModel>> GetWorkflowRuntimesAsync()
        {
            using var connection = new OracleConnection(ConnectionString);
            return (await Models.WorkflowRuntime.SelectAllAsync(connection).ConfigureAwait(false)).Select(GetModel).ToList();
        }

        private WorkflowRuntimeModel GetModel(Models.WorkflowRuntime result)
        {
            return new WorkflowRuntimeModel
            {
                Lock = result.LOCKFLAG,
                RuntimeId = result.RuntimeId,
                Status = result.Status,
                RestorerId = result.RestorerId,
                LastAliveSignal = result.LastAliveSignal,
                NextTimerTime = result.NextTimerTime
            };
        }
        
        #endregion

        #region ISchemePersistenceProvider

       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeByProcessIdAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessInstance processInstance = await WorkflowProcessInstance.SelectByKeyAsync(connection, processId).ConfigureAwait(false);
            if (processInstance == null)
            {
                throw new ProcessNotFoundException(processId);
            }

            if (!processInstance.SchemeId.HasValue)
            {
                throw SchemeNotFoundException.Create(processId, SchemeLocation.WorkflowProcessInstance);
            }

            SchemeDefinition<XElement> schemeDefinition = await GetProcessSchemeBySchemeIdAsync(processInstance.SchemeId.Value).ConfigureAwait(false);
            schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
            return schemeDefinition;
        }

       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeBySchemeIdAsync(Guid schemeId)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessScheme processScheme = await WorkflowProcessScheme.SelectByKeyAsync(connection, schemeId).ConfigureAwait(false);

            if (processScheme == null || String.IsNullOrEmpty(processScheme.Scheme))
            {
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
            }

            return ConvertToSchemeDefinition(processScheme);
        }

       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeWithParametersAsync(string schemeCode, string definingParameters,
            Guid? rootSchemeId, bool ignoreObsolete)
        {
            IEnumerable<WorkflowProcessScheme> processSchemes;
            string hash = HashHelper.GenerateStringHash(definingParameters);

            using (var connection = new OracleConnection(ConnectionString))
            {
                processSchemes =
                    await WorkflowProcessScheme.SelectAsync(connection, schemeCode, hash, ignoreObsolete ? false : (bool?)null,
                        rootSchemeId).ConfigureAwait(false);
            }

            if (!processSchemes.Any())
            {
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme, definingParameters);
            }

            if (processSchemes.Count() == 1)
            {
                WorkflowProcessScheme scheme = processSchemes.First();
                return ConvertToSchemeDefinition(scheme);
            }

            foreach (WorkflowProcessScheme processScheme in processSchemes.Where(processScheme => processScheme.DefiningParameters == definingParameters))
            {
                return ConvertToSchemeDefinition(processScheme);
            }

            throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme, definingParameters);
        }

       public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode, IDictionary<string, object> parameters)
        {
            string definingParameters = DefiningParametersSerializer.Serialize(parameters);
            string definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using var connection = new OracleConnection(ConnectionString);
            await WorkflowProcessScheme.SetObsoleteAsync(connection, schemeCode, definingParametersHash).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowProcessScheme.SetObsoleteAsync(connection, schemeCode).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task<SchemeDefinition<XElement>> SaveSchemeAsync(SchemeDefinition<XElement> scheme)
        {
            string definingParameters = scheme.DefiningParameters;
            string definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using var connection = new OracleConnection(ConnectionString);
            WorkflowProcessScheme[] oldSchemes = await WorkflowProcessScheme.SelectAsync(connection, scheme.SchemeCode, definingParametersHash, scheme.IsObsolete, scheme.RootSchemeId).ConfigureAwait(false);

            if (oldSchemes.Any())
            {
                WorkflowProcessScheme existing = oldSchemes.FirstOrDefault(oldScheme => oldScheme.DefiningParameters == definingParameters);
                if (existing != null)
                {
                    return ConvertToSchemeDefinition(existing);
                }
            }

            var newProcessScheme = new WorkflowProcessScheme
            {
                Id = scheme.Id,
                DefiningParameters = definingParameters,
                DefiningParametersHash = definingParametersHash,
                Scheme = scheme.Scheme.ToString(),
                SchemeCode = scheme.SchemeCode,
                RootSchemeCode = scheme.RootSchemeCode,
                RootSchemeId = scheme.RootSchemeId,
                AllowedActivities = JsonConvert.SerializeObject(scheme.AllowedActivities),
                StartingTransition = scheme.StartingTransition,
                IsObsolete = scheme.IsObsolete
            };

            await newProcessScheme.InsertAsync(connection).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);

            return ConvertToSchemeDefinition(newProcessScheme);
        }

        public virtual async Task SaveSchemeAsync(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowScheme wfScheme = await WorkflowScheme.SelectByKeyAsync(connection, schemaCode).ConfigureAwait(false);
            if (wfScheme == null)
            {
                wfScheme = new WorkflowScheme
                {
                    Code = schemaCode,
                    Scheme = scheme,
                    CanBeInlined = canBeInlined,
                    InlinedSchemes = inlinedSchemes.Any() ? JsonConvert.SerializeObject(inlinedSchemes) : null,
                    Tags = TagHelper.ToTagStringForDatabase(tags)
                };
               await wfScheme.InsertAsync(connection).ConfigureAwait(false);
            }
            else
            {
                wfScheme.Scheme = scheme;
                wfScheme.CanBeInlined = canBeInlined;
                wfScheme.InlinedSchemes = inlinedSchemes.Any() ? JsonConvert.SerializeObject(inlinedSchemes) : null;
                wfScheme.Tags = TagHelper.ToTagStringForDatabase(tags);
                await wfScheme.UpdateAsync(connection).ConfigureAwait(false);
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

        public virtual async Task<XElement> GetSchemeAsync(string code)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowScheme scheme = await WorkflowScheme.SelectByKeyAsync(connection, code).ConfigureAwait(false);
            if (scheme == null || String.IsNullOrEmpty(scheme.Scheme))
            {
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);
            }

            return XElement.Parse(scheme.Scheme);
        }

        public virtual async Task<List<string>> GetInlinedSchemeCodesAsync()
        {
            using var connection = new OracleConnection(ConnectionString);
            return await WorkflowScheme.GetInlinedSchemeCodesAsync(connection).ConfigureAwait(false);
        }

        public virtual async Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode)
        {
            using var connection = new OracleConnection(ConnectionString);
            return await WorkflowScheme.GetRelatedSchemeCodesAsync(connection, schemeCode).ConfigureAwait(false);
        }

       public virtual async Task<List<string>> SearchSchemesByTagsAsync(params string[] tags)
        {
            return await SearchSchemesByTagsAsync(tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags)
        {
            using var connection = new OracleConnection(ConnectionString);
            return await WorkflowScheme.GetSchemeCodesByTagsAsync(connection, tags).ConfigureAwait(false);
        }

       public virtual async Task AddSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await AddSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowScheme.AddSchemeTagsAsync(connection, schemeCode, tags, _runtime.Builder).ConfigureAwait(false);
        }

       public virtual async Task RemoveSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await RemoveSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowScheme.RemoveSchemeTagsAsync(connection, schemeCode, tags, _runtime.Builder).ConfigureAwait(false);
        }

       public virtual async Task SetSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await SetSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowScheme.SetSchemeTagsAsync(connection, schemeCode, tags, _runtime.Builder).ConfigureAwait(false);
        }

        #endregion

        #region IWorkflowGenerator

       public virtual async Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
            {
                throw new InvalidOperationException("Parameters not supported");
            }

            return await GetSchemeAsync(schemeCode).ConfigureAwait(false);
        }

        #endregion

        #region Bulk methods

        public bool IsBulkOperationsSupported => false;

#pragma warning disable 1998
       public virtual async Task BulkInitProcessesAsync(List<ProcessInstance> instances, ProcessStatus status, CancellationToken token)
#pragma warning restore 1998
        {
            throw new NotImplementedException();
        }

#pragma warning disable 1998
       public virtual async Task BulkInitProcessesAsync(List<ProcessInstance> instances, List<TimerToRegister> timers, ProcessStatus status, CancellationToken token)
#pragma warning restore 1998
        {
            throw new NotImplementedException();
        }

        #endregion

        private SchemeDefinition<XElement> ConvertToSchemeDefinition(WorkflowProcessScheme workflowProcessScheme)
        {
            return new SchemeDefinition<XElement>(workflowProcessScheme.Id, workflowProcessScheme.RootSchemeId,
                workflowProcessScheme.SchemeCode, workflowProcessScheme.RootSchemeCode,
                XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete, false,
                JsonConvert.DeserializeObject<List<string>>(workflowProcessScheme.AllowedActivities ?? "null"),
                workflowProcessScheme.StartingTransition,
                workflowProcessScheme.DefiningParameters);
        }

        private async Task<Tuple<int, WorkflowRuntimeModel>> UpdateWorkflowRuntimeAsync(WorkflowRuntimeModel runtime, Action<WorkflowRuntimeModel> setter,
            Func<OracleConnection, WorkflowRuntimeModel, Guid, Task<int>> updateMethod)
        {
            using var connection = new OracleConnection(ConnectionString);
            Guid oldLock = runtime.Lock;
            setter(runtime);
            runtime.Lock = Guid.NewGuid();

            int cnt = await updateMethod(connection, runtime, oldLock).ConfigureAwait(false);

            if (cnt != 1)
            {
                return new Tuple<int, WorkflowRuntimeModel>(cnt, null);
            }

            await DbObject.CommitAsync(connection).ConfigureAwait(false);

            return new Tuple<int, WorkflowRuntimeModel>(cnt, runtime);
        }

        #region IApprovalProvider

       public virtual async Task DropWorkflowInboxAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            await WorkflowInbox.DeleteByAsync(connection, x=>x.ProcessId, processId)
                .ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public virtual async Task InsertInboxAsync(List<InboxItem> newActors)
        {
            using var connection = new OracleConnection(ConnectionString);
            WorkflowInbox[] inboxItems = newActors.Select(WorkflowInbox.ToDB).ToArray();
            await WorkflowInbox.InsertAllAsync(connection, inboxItems).ConfigureAwait(false);
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public async Task<int> GetInboxCountByProcessIdAsync(Guid processId)
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowInbox.GetCountByAsync(connection, x => x.ProcessId, processId)
               .ConfigureAwait(false);
       }

       public async Task<int> GetInboxCountByIdentityIdAsync(string identityId)
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowInbox.GetCountByAsync(connection, x => x.IdentityId, identityId)
               .ConfigureAwait(false);
       }

       public async Task<List<InboxItem>> GetInboxByProcessIdAsync(Guid processId, Paging paging = null, CultureInfo culture = null)
       {
           using var connection = new OracleConnection(ConnectionString);
           WorkflowInbox[] inboxItems = await WorkflowInbox.SelectByWithPagingAsync(connection,
                   x => x.ProcessId, processId,
                   x => x.AddingDate, SortDirection.Desc,
                   paging)
               .ConfigureAwait(false);

           return await WorkflowInbox.FromDB(_runtime, inboxItems, culture ?? CultureInfo.CurrentCulture)
               .ConfigureAwait(false);
       }

       public async Task<List<InboxItem>> GetInboxByIdentityIdAsync(string identityId, Paging paging = null, CultureInfo culture = null)
       {
           using var connection = new OracleConnection(ConnectionString);
           WorkflowInbox[] inboxItems = await WorkflowInbox.SelectByWithPagingAsync(connection,
                   x => x.IdentityId, identityId,
                   x => x.AddingDate, SortDirection.Desc,
                   paging)
               .ConfigureAwait(false);
           
           return await WorkflowInbox.FromDB(_runtime, inboxItems, culture ?? CultureInfo.CurrentCulture)
               .ConfigureAwait(false);
       }
       
       public async Task FillApprovalHistoryAsync(ApprovalHistoryItem approvalHistoryItem)
       {
           using var connection = new OracleConnection(ConnectionString);
           WorkflowApprovalHistory historyItem = 
               (await WorkflowApprovalHistory.SelectByProcessIdAsync(connection, approvalHistoryItem.ProcessId).ConfigureAwait(false))
               .FirstOrDefault(h =>!h.TransitionTime.HasValue &&
                                   h.InitialState == approvalHistoryItem.InitialState && h.DestinationState == approvalHistoryItem.DestinationState);

           if (historyItem is null)
           {
               historyItem =  WorkflowApprovalHistory.ToDb(approvalHistoryItem);

               await historyItem.InsertAsync(connection).ConfigureAwait(false);
           }
           else
           {
               historyItem.TriggerName = approvalHistoryItem.TriggerName;
               historyItem.TransitionTime = approvalHistoryItem.TransitionTime;
               historyItem.IdentityId = approvalHistoryItem.IdentityId;
               historyItem.Commentary = approvalHistoryItem.Commentary;
               
               await historyItem.UpdateAsync(connection).ConfigureAwait(false);
           }

           await DbObject.CommitAsync(connection).ConfigureAwait(false);
       }

       public virtual async Task DropEmptyApprovalHistoryAsync(Guid processId)
        {
            using var connection = new OracleConnection(ConnectionString);
            foreach (WorkflowApprovalHistory record in (await WorkflowApprovalHistory.SelectByProcessIdAsync(connection, processId).ConfigureAwait(false)).Where(x => !x.TransitionTime.HasValue).ToList())
            {
                await WorkflowApprovalHistory.DeleteAsync(connection, record.Id).ConfigureAwait(false);
            }
            await DbObject.CommitAsync(connection).ConfigureAwait(false);
        }

       public async Task DropApprovalHistoryByProcessIdAsync(Guid processId)
       {
           using var connection = new OracleConnection(ConnectionString);
           await WorkflowApprovalHistory.DeleteByAsync(connection, x => x.ProcessId, processId)
               .ConfigureAwait(false);
           await DbObject.CommitAsync(connection).ConfigureAwait(false);
       }

       public async Task DropApprovalHistoryByIdentityIdAsync(string identityId)
       {
           using var connection = new OracleConnection(ConnectionString);
           await WorkflowApprovalHistory.DeleteByAsync(connection, x => x.IdentityId, identityId)
               .ConfigureAwait(false);
           await DbObject.CommitAsync(connection).ConfigureAwait(false);
       }

       public async Task<int> GetApprovalHistoryCountByProcessIdAsync(Guid processId)
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowApprovalHistory.GetCountByAsync(connection, x => x.ProcessId, processId)
               .ConfigureAwait(false);
       }

       public async Task<int> GetApprovalHistoryCountByIdentityIdAsync(string identityId)
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowApprovalHistory.GetCountByAsync(connection, x => x.IdentityId, identityId)
               .ConfigureAwait(false);
       }

       public async Task<List<ApprovalHistoryItem>> GetApprovalHistoryByProcessIdAsync(Guid processId, Paging paging = null)
       {
           using var connection = new OracleConnection(ConnectionString);

           return (await WorkflowApprovalHistory.SelectByWithPagingAsync(connection,
                   x=>x.ProcessId, processId, 
                   x=>x.Sort, SortDirection.Asc,
                   paging)
               .ConfigureAwait(false)).Select(WorkflowApprovalHistory.FromDb).ToList();
       }

       public async Task<List<ApprovalHistoryItem>> GetApprovalHistoryByIdentityIdAsync(string identityId, Paging paging = null)
       {
           using var connection = new OracleConnection(ConnectionString);
           
           return (await WorkflowApprovalHistory.SelectByWithPagingAsync(connection,
                   x=>x.IdentityId, identityId,
                   x=>x.Sort, SortDirection.Asc,
                   paging)
               .ConfigureAwait(false)).Select(WorkflowApprovalHistory.FromDb).ToList();
       }

       public async Task<int> GetOutboxCountByIdentityIdAsync(string identityId)
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowApprovalHistory.GetOutboxCountByIdentityIdAsync(connection, identityId)
               .ConfigureAwait(false);
       }

       public async Task<List<OutboxItem>> GetOutboxByIdentityIdAsync(string identityId, Paging paging = null)
       {
           using var connection = new OracleConnection(ConnectionString);
           return await WorkflowApprovalHistory.SelectOutboxByIdentityIdAsync(connection, identityId, paging)
               .ConfigureAwait(false);
       }
       
       #endregion IApprovalProvider
    }
}
