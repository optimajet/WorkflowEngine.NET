using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Runtime.Timers;
using OptimaJet.Workflow.MSSQL.Models;
using OptimaJet.Workflow.Plugins;
using WorkflowRuntime = OptimaJet.Workflow.Core.Runtime.WorkflowRuntime;
using WorkflowSync = OptimaJet.Workflow.MSSQL.Models.WorkflowSync;

namespace OptimaJet.Workflow.DbPersistence
{
    public class MSSQLProvider : IWorkflowProvider
    {
        public string ConnectionString { get; set; }
        private WorkflowRuntime _runtime;
        private readonly bool _writeToHistory;
        private readonly bool _writeSubProcessToRoot;

        public virtual void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public MSSQLProvider(string connectionString, string schema = "dbo", bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            ConnectionString = connectionString;
            DbObject.SchemaName = schema;

            _writeToHistory = writeToHistory;
            _writeSubProcessToRoot = writeSubProcessToRoot;
        }

        #region IPersistenceProvider
        
        #region IAssignmentProvider
        public async Task DeleteAssignmentAsync(Guid assignmentId)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowProcessAssignment.DeleteAsync(connection, assignmentId).ConfigureAwait(false);
        }

        public async Task<List<Assignment>> GetAssignmentsAsync(AssignmentFilter filter, List<(string parameterName,SortDirection sortDirection)> orderParameters = null, Paging paging = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            var assignments = await WorkflowProcessAssignment.SelectByFilterAsync(connection, filter.Parameters, orderParameters,  paging).ConfigureAwait(false);
            
            return assignments.Select(a => a.ConvertToAssignment()).ToList();
        }

        public async Task<int> GetAssignmentCountAsync(AssignmentFilter filter)
        {
            using var connection = new SqlConnection(ConnectionString);
            return  await WorkflowProcessAssignment.GetAssignmentCountAsync(connection, filter.Parameters).ConfigureAwait(false);
        }

        public async Task CreateAssignmentAsync(Guid processId, AssignmentCreationForm form)
        {
            using var connection = new SqlConnection(ConnectionString);
            form.Observers ??= new List<string>();
            form.Tags ??= new List<string>();
            
            var assignment = new WorkflowProcessAssignment
            {
                Id = form.Id ?? Guid.NewGuid(),
                AssignmentCode = form.AssignmentCode,
                Name = form.Name,
                Description = form.Description,
                Executor = form.Executor,
                ProcessId = processId,
                StatusState = AssignmentPlugin.DefaultStatus,
                IsDeleted = false,
                IsActive = form.IsActive,
                DeadlineToComplete = form.DeadlineToComplete,
                DeadlineToStart = form.DeadlineToStart,
                Observers = JsonConvert.SerializeObject(form.Observers),
                Tags = JsonConvert.SerializeObject(form.Tags),
                DateCreation = _runtime.RuntimeDateTimeNow
            };

            await assignment.InsertAsync(connection).ConfigureAwait(false);
        }

        public virtual async Task<Assignment> GetAssignmentAsync(Guid assignmentId)
        {
            using var connection = new SqlConnection(ConnectionString);
            var assignment = await WorkflowProcessAssignment.SelectByKeyAsync(connection, assignmentId).ConfigureAwait(false);
            
            return assignment.ConvertToAssignment();
        }
        
        public async Task UpdateAssignmentAsync(Assignment a)
        {
            using var connection = new SqlConnection(ConnectionString);
            var assignment = await WorkflowProcessAssignment.SelectByKeyAsync(connection, a.AssignmentId).ConfigureAwait(false);
            
            assignment.Name = a.Name;
            assignment.Description = a.Description;
            assignment.Executor = a.Executor;
            assignment.ProcessId = a.ProcessId;
            assignment.StatusState = a.StatusState;
            assignment.DateStart = a.DateStart;
            assignment.DateFinish = a.DateFinish;
            assignment.IsActive = a.IsActive;
            assignment.IsDeleted = a.IsDeleted;
            assignment.DeadlineToComplete = a.DeadlineToComplete;
            assignment.DeadlineToStart = a.DeadlineToStart;
            assignment.Observers = JsonConvert.SerializeObject(a.Observers ?? new List<string>());
            assignment.Tags = JsonConvert.SerializeObject(a.Tags ?? new List<string>());
            
            await assignment.UpdateAsync(connection).ConfigureAwait(false);
        }
        #endregion
        
        public async Task DeleteInactiveTimersByProcessIdAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowProcessTimer.DeleteInactiveByProcessIdAsync(connection, processId).ConfigureAwait(false);
        }

        public async Task DeleteTimerAsync(Guid timerId)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowProcessTimer.DeleteAsync(connection, timerId).ConfigureAwait(false);
        }

        public virtual async Task<List<Guid>> GetRunningProcessesAsync(string runtimeId = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowProcessInstanceStatus.GetProcessesByStatusAsync(connection, ProcessStatus.Running.Id, runtimeId).ConfigureAwait(false);
        }

        public virtual async Task<bool> MultiServerRuntimesExistAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            return await MSSQL.Models.WorkflowRuntime.MultiServerRuntimesExistAsync(connection).ConfigureAwait(false);
        }

        public virtual async Task<int> ActiveMultiServerRuntimesCountAsync(string currentRuntimeId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await MSSQL.Models.WorkflowRuntime.GetActiveMultiServerRuntimesCountAsync(connection, currentRuntimeId).ConfigureAwait(false);
        }

        public virtual async Task<WorkflowRuntimeModel> CreateWorkflowRuntimeAsync(string runtimeId, RuntimeStatus status)
        {
            using var connection = new SqlConnection(ConnectionString);
            
            var runtime = new MSSQL.Models.WorkflowRuntime()
            {
                RuntimeId = runtimeId,
                Lock = Guid.NewGuid(),
                Status = status
            };

            await runtime.InsertAsync(connection).ConfigureAwait(false);

            return new WorkflowRuntimeModel { Lock = runtime.Lock, RuntimeId = runtimeId, Status = status };
        }

        public virtual async Task DeleteWorkflowRuntimeAsync(string name)
        {
            using var connection = new SqlConnection(ConnectionString);
            await MSSQL.Models.WorkflowRuntime.DeleteAsync(connection, name).ConfigureAwait(false);
        }

        public async Task<List<ProcessInstanceItem>> GetProcessInstancesAsync(List<(string parameterName,SortDirection sortDirection)> orderParameters = null, Paging paging = null)
        {
            using var connection = new SqlConnection(ConnectionString);
           
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
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowProcessInstance.GetCountAsync(connection).ConfigureAwait(false);
        }

        public async Task<List<SchemeItem>> GetSchemesAsync(List<(string parameterName,SortDirection sortDirection)> orderParameters = null, Paging paging = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            
            WorkflowScheme[] schemes = await WorkflowScheme.SelectAllWorkflowSchemesWithPagingAsync(connection, orderParameters, paging).ConfigureAwait(false);
           
            return schemes.Select(sc => new SchemeItem()
            {
                Code = sc.Code,
                Scheme = sc.Scheme,
                CanBeInlined = sc.CanBeInlined,
                InlinedSchemes = sc.GetInlinedSchemes(),
                Tags = TagHelper.FromTagString(sc.Tags),
            }).ToList();
        }
       
        public async Task<int> GetSchemesCountAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowScheme.GetCountAsync(connection).ConfigureAwait(false);
        }

        public virtual async Task<WorkflowRuntimeModel> UpdateWorkflowRuntimeStatusAsync(WorkflowRuntimeModel runtime, RuntimeStatus status)
        {
            Tuple<int, WorkflowRuntimeModel> res = await UpdateWorkflowRuntimeAsync(runtime, x => x.Status = status, MSSQL.Models.WorkflowRuntime.UpdateStatusAsync).ConfigureAwait(false);

            if (res.Item1 != 1)
            {
                throw new ImpossibleToSetRuntimeStatusException();
            }

            return res.Item2;
        }

        public virtual async Task<(bool Success, WorkflowRuntimeModel UpdatedModel)> UpdateWorkflowRuntimeRestorerAsync(WorkflowRuntimeModel runtime, string restorerId)
        {
            Tuple<int, WorkflowRuntimeModel> res = await UpdateWorkflowRuntimeAsync(runtime, x => x.RestorerId = restorerId, MSSQL.Models.WorkflowRuntime.UpdateRestorerAsync).ConfigureAwait(false);

            return (res.Item1 == 1, res.Item2);
        }

        public virtual async Task InitializeProcessAsync(ProcessInstance processInstance)
        {
            using var connection = new SqlConnection(ConnectionString);
            
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
        }

        public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance)
        {
            await BindProcessToNewSchemeAsync(processInstance, false).ConfigureAwait(false);
        }

        public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            using var connection = new SqlConnection(ConnectionString);
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
                                                                           .Select(ptp => ParameterDefinitionWithValueToDynamic(ptp))
                                                                           .ToList();
            
            using var connection = new SqlConnection(ConnectionString);
            var persistedParameters = (await WorkflowProcessInstancePersistence.SelectByProcessIdAsync(connection, processInstance.ProcessId).ConfigureAwait(false)).ToList();

            foreach (dynamic parameterDefinitionWithValue in parametersToPersistList)
            {
                WorkflowProcessInstancePersistence persistence = persistedParameters.SingleOrDefault(pp => pp.ParameterName == parameterDefinitionWithValue.Parameter.Name);

                await InsertOrUpdateParameterAsync(connection, processInstance, parameterDefinitionWithValue, persistence).ConfigureAwait(false);
            }
        }
        public virtual async Task SavePersistenceParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            dynamic parameter = ParameterDefinitionWithValueToDynamic(processInstance.ProcessParameters.Single(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Name == parameterName));

            using var connection = new SqlConnection(ConnectionString);
            WorkflowProcessInstancePersistence persistedParameter = await WorkflowProcessInstancePersistence.SelectByNameAsync(connection, processInstance.ProcessId, parameterName).ConfigureAwait(false);

            await InsertOrUpdateParameterAsync(connection, processInstance, parameter, persistedParameter).ConfigureAwait(false);
        }
        
        private dynamic ParameterDefinitionWithValueToDynamic(ParameterDefinitionWithValue ptp)
        {
            string serializedValue = ptp.Type == typeof(UnknownParameterType) ? (string)ptp.Value : ParametersSerializer.Serialize(ptp.Value, ptp.Type);
            return new { Parameter = ptp, SerializedValue = serializedValue };
        }
        
        private async Task InsertOrUpdateParameterAsync(SqlConnection connection, ProcessInstance processInstance, dynamic parameter, WorkflowProcessInstancePersistence persistence)
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
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowProcessInstancePersistence.DeleteByNameAsync(connection, processInstance.ProcessId, parameterName).ConfigureAwait(false);
        }

        public virtual async Task SetProcessStatusAsync(Guid processId, ProcessStatus newStatus)
        {
            if (newStatus == ProcessStatus.Running)
            {
                await SetRunningStatusAsync(processId).ConfigureAwait(false);
            }
            else
            {
                await SetCustomStatusAsync(processId,newStatus).ConfigureAwait(false);
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

            using var connection = new SqlConnection(ConnectionString);

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

            using var connection = new SqlConnection(ConnectionString);
            
            WorkflowProcessInstance inst = await WorkflowProcessInstance.SelectByKeyAsync(connection, processInstance.ProcessId).ConfigureAwait(false);

            if (!(inst == null || transition.To.DisablePersistState))
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

            if (!_writeToHistory || transition.To.DisablePersistTransitionHistory)
            {
                return;
            }

            string actorName = null;
            string executorName = null;
            if (_runtime.GetUserByIdentityAsync != null)
            {
                if (!String.IsNullOrEmpty(impIdentityId) )
                {
                    actorName = await _runtime.GetUserByIdentityAsync(impIdentityId).ConfigureAwait(false);
                }
                if (!String.IsNullOrEmpty(identityId))
                {
                    executorName = await _runtime.GetUserByIdentityAsync(identityId).ConfigureAwait(false);
                }
            }

            var history = new WorkflowProcessTransitionHistory
            {
                ActorIdentityId = impIdentityId,
                ExecutorIdentityId = identityId,
                ActorName = actorName,
                ExecutorName = executorName,
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

        public virtual async Task<bool> IsProcessExistsAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            
            return await WorkflowProcessInstance.SelectByKeyAsync(connection, processId).ConfigureAwait(false) != null;
        }

        public virtual async Task<ProcessStatus> GetInstanceStatusAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
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
            using var connection = new SqlConnection(ConnectionString);
            
            WorkflowProcessInstanceStatus instanceStatus = await WorkflowProcessInstanceStatus.SelectByKeyAsync(connection, processId).ConfigureAwait(false);

            if (instanceStatus == null)
            {
                throw new StatusNotDefinedException();
            }

            if (instanceStatus.Status == ProcessStatus.Running.Id)
            {
                throw new ImpossibleToSetStatusException("Process already running");
            }

            Guid oldLock = instanceStatus.Lock;

            instanceStatus.Lock = Guid.NewGuid();
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
        }

        private async Task SetCustomStatusAsync(Guid processId, ProcessStatus status, bool createIfNotDefined = false)
        {
            using var connection = new SqlConnection(ConnectionString);
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
                    Lock = Guid.NewGuid(),
                    Status = status.Id,
                    RuntimeId = _runtime.Id,
                    SetTime = _runtime.RuntimeDateTimeNow
                };

                await instanceStatus.InsertAsync(connection).ConfigureAwait(false);
            }
            else
            {
                Guid oldLock = instanceStatus.Lock;

                instanceStatus.Status = status.Id;
                instanceStatus.Lock = Guid.NewGuid();
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
        }

        private async Task<List<ParameterDefinitionWithValue>> GetProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
        {
            var parameters = new List<ParameterDefinitionWithValue>(processDefinition.Parameters.Count);
            parameters.AddRange(await GetPersistedProcessParametersAsync(processId, processDefinition).ConfigureAwait(false));
            parameters.AddRange(await GetSystemProcessParametersAsync(processId, processDefinition).ConfigureAwait(false));
            return parameters;
        }

        private async Task<List<ParameterDefinitionWithValue>> GetSystemProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
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

        private async Task<List<ParameterDefinitionWithValue>> GetPersistedProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count);

            List<WorkflowProcessInstancePersistence> persistedParameters;

            using (var connection = new SqlConnection(ConnectionString))
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
            using (var connection = new SqlConnection(ConnectionString))
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
            using var connection = new SqlConnection(ConnectionString);
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

        public virtual async Task SaveGlobalParameterAsync<T>(string type, string name, T value)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowGlobalParameter parameter = (await WorkflowGlobalParameter.SelectByTypeAndNameAsync(connection, type, name).ConfigureAwait(false)).FirstOrDefault();

            if (parameter == null)
            {
                parameter = new WorkflowGlobalParameter()
                {
                    Id = Guid.NewGuid(),
                    Type = type,
                    Name = name,
                    Value = JsonConvert.SerializeObject(value)
                };

                await parameter.InsertAsync(connection).ConfigureAwait(false);
            }
            else
            {
                parameter.Value = JsonConvert.SerializeObject(value);

                await parameter.UpdateAsync(connection).ConfigureAwait(false);
            }
        }

        public virtual async Task<T> LoadGlobalParameterAsync<T>(string type, string name)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowGlobalParameter parameter = (await WorkflowGlobalParameter.SelectByTypeAndNameAsync(connection, type, name).ConfigureAwait(false)).FirstOrDefault();

            if (parameter == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(parameter.Value);
        }

        public async Task<Dictionary<string, T>> LoadGlobalParametersWithNamesAsync<T>(string type)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowGlobalParameter[] parameters = await WorkflowGlobalParameter.SelectByTypeAndNameAsync(connection, type).ConfigureAwait(false);

            var dict = new Dictionary<string, T>();
            foreach (var parameter in parameters)
            {
                dict[parameter.Name] = JsonConvert.DeserializeObject<T>(parameter.Value);
            }

            return dict;
        }

        public virtual async Task<List<T>> LoadGlobalParametersAsync<T>(string type)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowGlobalParameter[] parameters = await WorkflowGlobalParameter.SelectByTypeAndNameAsync(connection, type).ConfigureAwait(false);

            return parameters.Select(p => JsonConvert.DeserializeObject<T>(p.Value)).ToList();
        }
       
        public virtual async Task DeleteGlobalParametersAsync(string type, string name = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowGlobalParameter.DeleteByTypeAndNameAsync(connection, type, name).ConfigureAwait(false);
        }

        public virtual async Task DeleteProcessAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using SqlTransaction transaction = connection.BeginTransaction();
            await WorkflowProcessInstance.DeleteAsync(connection, processId, transaction).ConfigureAwait(false);
            await WorkflowProcessInstanceStatus.DeleteAsync(connection, processId, transaction).ConfigureAwait(false);
            await WorkflowProcessInstancePersistence.DeleteByProcessIdAsync(connection, processId, transaction).ConfigureAwait(false);
            await WorkflowProcessTransitionHistory.DeleteByProcessIdAsync(connection, processId, transaction).ConfigureAwait(false);
            await WorkflowProcessTimer.DeleteByProcessIdAsync(connection, processId, null, transaction).ConfigureAwait(false);
            await WorkflowInbox.DeleteByProcessIdAsync(connection, processId, transaction).ConfigureAwait(false);
            await WorkflowApprovalHistory.DeleteByProcessIdAsync(connection, processId, transaction).ConfigureAwait(false);
            await WorkflowProcessAssignment.DeleteByProcessIdAsync(connection, processId, transaction).ConfigureAwait(false);
            
            transaction.Commit();
        }

        public virtual async Task RegisterTimerAsync(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowProcessTimer timer = await WorkflowProcessTimer.SelectByProcessIdAndNameAsync(connection, processId, name).ConfigureAwait(false);
            if (timer == null)
            {
                timer = new WorkflowProcessTimer
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    NextExecutionDateTime = nextExecutionDateTime,
                    ProcessId = processId,
                    Ignore = false,
                    RootProcessId = rootProcessId
                };

                await timer.InsertAsync(connection).ConfigureAwait(false);
            }
            else if (!notOverrideIfExists)
            {
                timer.NextExecutionDateTime = nextExecutionDateTime;
                await timer.UpdateAsync(connection).ConfigureAwait(false);
            }
        }

        public virtual async Task ClearTimersAsync(Guid processId, List<string> timersIgnoreList)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowProcessTimer.DeleteByProcessIdAsync(connection, processId, timersIgnoreList).ConfigureAwait(false);
        }

        public virtual async Task<int> SetTimerIgnoreAsync(Guid timerId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowProcessTimer.SetTimerIgnoreAsync(connection, timerId).ConfigureAwait(false);
        }

        public virtual async Task<List<Core.Model.WorkflowTimer>> GetTopTimersToExecuteAsync(int top)
        {
            DateTime now = _runtime.RuntimeDateTimeNow;

            using var connection = new SqlConnection(ConnectionString);
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
                RootProcessId = t.RootProcessId,
            }).ToList();
        }

       public virtual async Task<List<ProcessHistoryItem>> GetProcessHistoryAsync(Guid processId, Paging paging = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            return (await WorkflowProcessTransitionHistory.SelectByProcessIdAsync(connection, processId, paging).ConfigureAwait(false))
                .Select(hi => new ProcessHistoryItem
                {
                    ActorIdentityId = hi.ActorIdentityId,
                    ExecutorIdentityId = hi.ExecutorIdentityId,
                    ActorName = hi.ActorName,
                    ExecutorName = hi.ExecutorName,
                    FromActivityName = hi.FromActivityName,
                    FromStateName = hi.FromStateName,
                    IsFinalised = hi.IsFinalised,
                    ProcessId = hi.ProcessId,
                    ToActivityName = hi.ToActivityName,
                    ToStateName = hi.ToStateName,
                    TransitionClassifier = (TransitionClassifier) Enum.Parse(typeof(TransitionClassifier), hi.TransitionClassifier),
                    TransitionTime = hi.TransitionTime,
                    TriggerName = hi.TriggerName,
                    StartTransitionTime = hi.StartTransitionTime,
                    TransitionDuration = hi.TransitionDuration
                })
                .ToList();
        }

       public async Task<int> GetProcessHistoryCountAsync(Guid processId)
       {
           using var connection = new SqlConnection(ConnectionString);
           return await WorkflowProcessTransitionHistory.GetCountByAsync(connection, x => x.ProcessId, processId)
               .ConfigureAwait(false);
       }

       public virtual async Task<List<ProcessTimer>> GetTimersForProcessAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            IEnumerable<WorkflowProcessTimer> timers = await WorkflowProcessTimer.SelectByProcessIdAsync(connection, processId).ConfigureAwait(false);
            return timers.Select(t => new ProcessTimer(t.Id, t.Name, t.NextExecutionDateTime)).ToList();
        }

        public virtual async Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await ProcessInstanceTreeItem.GetProcessTreeItemsByRootProcessIdAsync(connection, rootProcessId).ConfigureAwait(false);
        }

        public virtual async Task<List<ProcessTimer>> GetActiveTimersForProcessAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            IEnumerable<WorkflowProcessTimer> timers = await WorkflowProcessTimer.SelectActiveByProcessIdAsync(connection, processId).ConfigureAwait(false);
            return timers.Select(t => new ProcessTimer(t.Id, t.Name, t.NextExecutionDateTime)).ToList();
        }

        public virtual async Task<WorkflowRuntimeModel> GetWorkflowRuntimeModelAsync(string runtimeId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await MSSQL.Models.WorkflowRuntime.GetWorkflowRuntimeStatusAsync(connection, runtimeId).ConfigureAwait(false);
        }

        public virtual async Task<int> SendRuntimeLastAliveSignalAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            return await MSSQL.Models.WorkflowRuntime.SendRuntimeLastAliveSignalAsync(connection, _runtime.Id, _runtime.RuntimeDateTimeNow).ConfigureAwait(false);
        }

        public virtual async Task<DateTime?> GetNextTimerDateAsync(TimerCategory timerCategory, int timerInterval)
        {
            using var connection = new SqlConnection(ConnectionString);
            
            string categoryName = timerCategory.ToString();

            WorkflowSync syncLock = await WorkflowSync.GetByNameAsync(connection, categoryName).ConfigureAwait(false);

            if (syncLock == null)
            {
                throw new Exception($"Sync lock {categoryName} not found");
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
                    throw new Exception($"Unknown sync lock name: {categoryName}");
            }

            DateTime? max = await MSSQL.Models.WorkflowRuntime.GetMaxNextTimeAsync(connection, _runtime.Id, nextTimeColumnName).ConfigureAwait(false);

            DateTime result = _runtime.RuntimeDateTimeNow;

            if (max > result)
            {
                result = max.Value;
            }
                
            result += TimeSpan.FromMilliseconds(timerInterval);
                
            var newLock = Guid.NewGuid();
            
            using SqlTransaction transaction = connection.BeginTransaction();
            await MSSQL.Models.WorkflowRuntime.UpdateNextTimeAsync(connection, _runtime.Id, nextTimeColumnName, result, transaction).ConfigureAwait(false);
                   
            int rowCount = await WorkflowSync.UpdateLockAsync(connection, categoryName, syncLock.Lock, newLock, transaction).ConfigureAwait(false);

            if (rowCount == 0)
            {
                transaction.Rollback();
                return null;
            }
                    
            transaction.Commit();

            return result;
        }

        public virtual async Task<List<WorkflowRuntimeModel>> GetWorkflowRuntimesAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            return (await MSSQL.Models.WorkflowRuntime.SelectAllAsync(connection).ConfigureAwait(false)).Select(GetModel).ToList();
        }


        private WorkflowRuntimeModel GetModel(MSSQL.Models.WorkflowRuntime result)
        {
            return new WorkflowRuntimeModel { Lock = result.Lock, RuntimeId = result.RuntimeId, Status = result.Status, RestorerId = result.RestorerId, LastAliveSignal = result.LastAliveSignal, NextTimerTime = result.NextTimerTime };
        }

        #endregion

        #region ISchemePersistenceProvider

        public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeByProcessIdAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
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
            using var connection = new SqlConnection(ConnectionString);
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

            using (var connection = new SqlConnection(ConnectionString))
            {
                processSchemes =
                    await WorkflowProcessScheme.SelectAsync(connection, schemeCode, hash, ignoreObsolete ? false : (bool?) null,
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

            using var connection = new SqlConnection(ConnectionString);
            await WorkflowProcessScheme.SetObsoleteAsync(connection, schemeCode, definingParametersHash).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowProcessScheme.SetObsoleteAsync(connection, schemeCode).ConfigureAwait(false);
        }
        
        public virtual async Task DropUnusedWorkflowProcessSchemeAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            
            await WorkflowProcessScheme.DeleteUnusedAsync(connection).ConfigureAwait(false);
        }
        
        public virtual async Task<SchemeDefinition<XElement>> SaveSchemeAsync(SchemeDefinition<XElement> scheme)
        {
            string definingParameters = scheme.DefiningParameters;
            string definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using var connection = new SqlConnection(ConnectionString);
            WorkflowProcessScheme[] oldSchemes = await WorkflowProcessScheme.SelectAsync(connection, scheme.SchemeCode, definingParametersHash,
                scheme.IsObsolete, scheme.RootSchemeId).ConfigureAwait(false);

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

            return ConvertToSchemeDefinition(newProcessScheme);
        }

        public virtual async Task UpsertSchemeAsync(SchemeDefinition<XElement> scheme)
        {
            string definingParameters = scheme.DefiningParameters;
            string definingParametersHash = HashHelper.GenerateStringHash(definingParameters);
            
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
            
            using var connection = new SqlConnection(ConnectionString);
            try
            {
                await newProcessScheme.InsertAsync(connection).ConfigureAwait(false);
            }
            catch (Exception)
            {
                var existingSchemeCount = await WorkflowProcessScheme.GetCountByAsync(connection, s => s.Id, newProcessScheme.Id).ConfigureAwait(false);

                if (existingSchemeCount == 1)
                {
                    await newProcessScheme.UpdateAsync(connection).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        public virtual async Task SaveSchemeAsync(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme,
            List<string> tags)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowScheme wfScheme = await WorkflowScheme.SelectByKeyAsync(connection, schemaCode).ConfigureAwait(false);
            if (wfScheme == null)
            {
                wfScheme = new WorkflowScheme
                {
                    Code = schemaCode,
                    Scheme = scheme,
                    CanBeInlined = canBeInlined,
                    InlinedSchemes = inlinedSchemes.Any()
                        ? JsonConvert.SerializeObject(inlinedSchemes)
                        : null,
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
        }

        public virtual async Task<XElement> GetSchemeAsync(string code)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowScheme scheme = await WorkflowScheme.SelectByKeyAsync(connection, code).ConfigureAwait(false);
            if (scheme == null || String.IsNullOrEmpty(scheme.Scheme))
            {
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowProcessScheme);
            }

            return XElement.Parse(scheme.Scheme);
        }

        public virtual async Task<List<string>> GetInlinedSchemeCodesAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowScheme.GetInlinedSchemeCodesAsync(connection).ConfigureAwait(false);
        }

        public virtual async Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowScheme.GetRelatedSchemeCodesAsync(connection,schemeCode).ConfigureAwait(false);
        }

        public virtual async Task<List<string>> SearchSchemesByTagsAsync(params string[] tags)
        {
            return await SearchSchemesByTagsAsync(tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowScheme.GetSchemeCodesByTagsAsync(connection, tags).ConfigureAwait(false);
        }

        public virtual async Task AddSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await AddSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowScheme.AddSchemeTagsAsync(connection, schemeCode, tags, _runtime.Builder).ConfigureAwait(false);
        }

        public virtual async Task RemoveSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await RemoveSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowScheme.RemoveSchemeTagsAsync(connection, schemeCode, tags, _runtime.Builder).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await SetSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            using var connection = new SqlConnection(ConnectionString);
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

        private SchemeDefinition<XElement> ConvertToSchemeDefinition(WorkflowProcessScheme workflowProcessScheme)
        {
            return new SchemeDefinition<XElement>(workflowProcessScheme.Id, workflowProcessScheme.RootSchemeId,
                workflowProcessScheme.SchemeCode, workflowProcessScheme.RootSchemeCode,
                XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete, false,
                JsonConvert.DeserializeObject<List<string>>(workflowProcessScheme.AllowedActivities ?? "null"),
                workflowProcessScheme.StartingTransition,
                workflowProcessScheme.DefiningParameters);
        }

        private async Task<Tuple<int,WorkflowRuntimeModel>> UpdateWorkflowRuntimeAsync(WorkflowRuntimeModel runtime, Action<WorkflowRuntimeModel> setter, 
            Func<SqlConnection, WorkflowRuntimeModel, Guid, Task<int>> updateMethod)
        {
            using var connection = new SqlConnection(ConnectionString);
            
            Guid oldLock = runtime.Lock;
            
            setter(runtime);
            
            runtime.Lock = Guid.NewGuid();
            
            int cnt = await updateMethod(connection, runtime, oldLock).ConfigureAwait(false);
                
            if (cnt != 1)
            {
                return new Tuple<int, WorkflowRuntimeModel>(cnt,null);
            }
                
            return new Tuple<int, WorkflowRuntimeModel>(cnt,runtime);
        }

        #region Bulk methods

        public bool IsBulkOperationsSupported
        {
#if !NETCOREAPP || NETCORE2
            get { return true; }
#else
            get { return false; }
#endif

        }

        public virtual async Task BulkInitProcessesAsync(List<ProcessInstance> instances, ProcessStatus status, CancellationToken token)
        {
            await BulkInitProcessesAsync(instances, null, status, token).ConfigureAwait(false);
        }

        public virtual async Task BulkInitProcessesAsync(List<ProcessInstance> instances, List<TimerToRegister> timers, ProcessStatus status, CancellationToken token)
        {
#if NETCOREAPP && !NETCORE2
            throw new NotImplementedException();
#else
            if (token.IsCancellationRequested)
            {
                return;
            }

            bool needRegisterTimers = timers != null && timers.Any();

            var piDataTable = WorkflowProcessInstance.ToDataTable();
            var psDataTable = WorkflowProcessInstanceStatus.ToDataTable();
            var ppDataTable = WorkflowProcessInstancePersistence.ToDataTable();
            var ptDataTable = WorkflowProcessTimer.ToDataTable();

            foreach (ProcessInstance processInstance in instances)
            {
                DataRow processRow = piDataTable.NewRow();
                processRow["Id"] = processInstance.ProcessId;
                processRow["SchemeId"] = processInstance.SchemeId;
                processRow["ActivityName"] = processInstance.ProcessScheme.InitialActivity.Name;
                processRow["StateName"] = processInstance.ProcessScheme.InitialActivity.State;
                processRow["RootProcessId"] = processInstance.RootProcessId;
                processRow["TenantId"] = processInstance.TenantId;
                if (processInstance.ParentProcessId.HasValue)
                {
                    processRow["ParentProcessId"] = processInstance.ParentProcessId;
                }
                piDataTable.Rows.Add(processRow);
                DataRow statusRow = psDataTable.NewRow();
                statusRow["Id"] = processInstance.ProcessId;
                statusRow["Lock"] = Guid.NewGuid();
                statusRow["Status"] = status.Id;
                statusRow["RuntimeId"] = _runtime.Id;
                statusRow["SetTime"] = _runtime.RuntimeDateTimeNow;
                psDataTable.Rows.Add(statusRow);

                var parametersToPersist = processInstance.ProcessParameters.Where(p => p.Purpose == ParameterPurpose.Persistence && p.Value != null).ToList();

                foreach (ParameterDefinitionWithValue parameter in parametersToPersist)
                {
                    DataRow parameterRow = ppDataTable.NewRow();
                    parameterRow["Id"] = Guid.NewGuid();
                    parameterRow["ProcessId"] = processInstance.ProcessId;
                    parameterRow["ParameterName"] = parameter.Name;
                    parameterRow["Value"] = ParametersSerializer.Serialize(parameter.Value, parameter.Type);
                    ppDataTable.Rows.Add(parameterRow);
                }
            }

            if (needRegisterTimers)
            {
                foreach (TimerToRegister timer in timers)
                {
                    DataRow timerRow = ptDataTable.NewRow();
                    timerRow["Id"] = Guid.NewGuid();
                    timerRow["ProcessId"] = timer.ProcessId;
                    timerRow["RootProcessId"] = timer.ProcessId;
                    timerRow["Name"] = timer.Name;
                    timerRow["NextExecutionDateTime"] = timer.ExecutionDateTime;
                    timerRow["Ignore"] = false;
                    ptDataTable.Rows.Add(timerRow);
                }
            }

            using var connection = new SqlConnection(ConnectionString);
            // ReSharper disable once MethodSupportsCancellation
            await connection.OpenAsync().ConfigureAwait(false);
            using SqlTransaction transaction = connection.BeginTransaction();
            var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction) {DestinationTableName = WorkflowProcessInstance.ObjectName};
            bulk.ColumnMappings.Add("Id", "Id");
            bulk.ColumnMappings.Add("SchemeId", "SchemeId");
            bulk.ColumnMappings.Add("ActivityName", "ActivityName");
            bulk.ColumnMappings.Add("StateName", "StateName");
            bulk.ColumnMappings.Add("RootProcessId", "RootProcessId");
            bulk.ColumnMappings.Add("ParentProcessId", "ParentProcessId");
            bulk.ColumnMappings.Add("TenantId", "TenantId");
            await bulk.WriteToServerAsync(piDataTable, token).ConfigureAwait(false);
            bulk.DestinationTableName = WorkflowProcessInstanceStatus.ObjectName;
            bulk.ColumnMappings.Clear();
            bulk.ColumnMappings.Add("Id", "Id");
            bulk.ColumnMappings.Add("Lock", "Lock");
            bulk.ColumnMappings.Add("Status", "Status");
            bulk.ColumnMappings.Add("RuntimeId", "RuntimeId");
            bulk.ColumnMappings.Add("SetTime", "SetTime");
            await bulk.WriteToServerAsync(psDataTable, token).ConfigureAwait(false);
            if (ppDataTable.Rows.Count > 0)
            {
                bulk.DestinationTableName = WorkflowProcessInstancePersistence.ObjectName;
                bulk.ColumnMappings.Clear();
                bulk.ColumnMappings.Add("Id", "Id");
                bulk.ColumnMappings.Add("ProcessId", "ProcessId");
                bulk.ColumnMappings.Add("ParameterName", "ParameterName");
                bulk.ColumnMappings.Add("Value", "Value");
                await bulk.WriteToServerAsync(ppDataTable, token).ConfigureAwait(false);
            }
            if (needRegisterTimers)
            {
                bulk.DestinationTableName = WorkflowProcessTimer.ObjectName;
                bulk.ColumnMappings.Clear();
                bulk.ColumnMappings.Add("Id", "Id");
                bulk.ColumnMappings.Add("ProcessId", "ProcessId");
                bulk.ColumnMappings.Add("RootProcessId", "RootProcessId");
                bulk.ColumnMappings.Add("Name", "Name");
                bulk.ColumnMappings.Add("NextExecutionDateTime", "NextExecutionDateTime");
                bulk.ColumnMappings.Add("Ignore", "Ignore");
                await bulk.WriteToServerAsync(ptDataTable, token).ConfigureAwait(false);
            }

            if (token.IsCancellationRequested)
            {
                transaction.Rollback();
            }
            else
            {
                transaction.Commit();
            }

#endif
        }

        #endregion

        #region IApprovalProvider

        public virtual async Task DropWorkflowInboxAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowInbox.DeleteByAsync(connection, x=>x.ProcessId, processId)
                .ConfigureAwait(false);
        }

        public virtual async Task InsertInboxAsync(List<InboxItem> newActors)
        {
            using var connection = new SqlConnection(ConnectionString);
            WorkflowInbox[] inboxItems = newActors.Select(WorkflowInbox.ToDB).ToArray();
            await WorkflowInbox.InsertAllAsync(connection, inboxItems).ConfigureAwait(false);
        }

        public async Task<int> GetInboxCountByProcessIdAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowInbox.GetCountByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }

        public async Task<int> GetInboxCountByIdentityIdAsync(string identityId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowInbox.GetCountByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false);
        }

        public virtual async Task<List<InboxItem>> GetInboxByProcessIdAsync(Guid processId, Paging paging=null, CultureInfo culture = null)
        {
            using var connection = new SqlConnection(ConnectionString);
           
            WorkflowInbox[] inboxItems = await WorkflowInbox.SelectByWithPagingAsync(connection,
                    x => x.ProcessId, processId,
                    x => x.AddingDate, SortDirection.Desc,
                    paging)
                .ConfigureAwait(false);

            return await WorkflowInbox.FromDB(_runtime, inboxItems, culture ?? CultureInfo.CurrentCulture)
                .ConfigureAwait(false);
        }
       
        public virtual async Task<List<InboxItem>> GetInboxByIdentityIdAsync(string identityId, Paging paging=null, CultureInfo culture = null)
        {
            using var connection = new SqlConnection(ConnectionString);

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
            using var connection = new SqlConnection(ConnectionString);
            WorkflowApprovalHistory historyItem = 
                (await WorkflowApprovalHistory.SelectByProcessIdAsync(connection, approvalHistoryItem.ProcessId).ConfigureAwait(false))
                .FirstOrDefault(h =>!h.TransitionTime.HasValue &&
                                    h.InitialState == approvalHistoryItem.InitialState && h.DestinationState == approvalHistoryItem.DestinationState);

            if (historyItem is null)
            {
                historyItem =  WorkflowApprovalHistory.ToDB(approvalHistoryItem);

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
        }

        public async Task DropApprovalHistoryByIdentityIdAsync(string identityId)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowApprovalHistory.DeleteByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false);
        }

        public virtual async Task DropEmptyApprovalHistoryAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            foreach (WorkflowApprovalHistory record in (await WorkflowApprovalHistory.
                    SelectByProcessIdAsync(connection, processId).ConfigureAwait(false)).
                Where(x => !x.TransitionTime.HasValue).ToList())
            {
                await WorkflowApprovalHistory.DeleteAsync(connection, record.Id).ConfigureAwait(false);
            }
        }

        public async Task DropApprovalHistoryByProcessIdAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            await WorkflowApprovalHistory.DeleteByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }
       
        public async Task<int> GetApprovalHistoryCountByProcessIdAsync(Guid processId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowApprovalHistory.GetCountByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }

        public async Task<int> GetApprovalHistoryCountByIdentityIdAsync(string identityId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowApprovalHistory.GetCountByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false);
        }

        public virtual async  Task<List<ApprovalHistoryItem>> GetApprovalHistoryByProcessIdAsync(Guid processId, Paging paging=null)
        {
            using var connection = new SqlConnection(ConnectionString);

            return (await WorkflowApprovalHistory.SelectByWithPagingAsync(connection,
                    x=>x.ProcessId, processId, 
                    x=>x.Sort, SortDirection.Asc,
                    paging)
                .ConfigureAwait(false)).Select(WorkflowApprovalHistory.FromDB).ToList();
        }

        public virtual async  Task<List<ApprovalHistoryItem>> GetApprovalHistoryByIdentityIdAsync(string identityId, Paging paging=null)
        {
            using var connection = new SqlConnection(ConnectionString);
           
            return (await WorkflowApprovalHistory.SelectByWithPagingAsync(connection,
                    x=>x.IdentityId, identityId,
                    x=>x.Sort, SortDirection.Asc,
                    paging)
                .ConfigureAwait(false)).Select(WorkflowApprovalHistory.FromDB).ToList();
        }
       

        public async Task<int> GetOutboxCountByIdentityIdAsync(string identityId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowApprovalHistory.GetOutboxCountByIdentityIdAsync(connection, identityId)
                .ConfigureAwait(false);
        }
       
        public virtual async Task<List<OutboxItem>> GetOutboxByIdentityIdAsync(string identityId, Paging paging=null)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await WorkflowApprovalHistory.SelectOutboxByIdentityIdAsync(connection, identityId, paging)
                .ConfigureAwait(false);
        }

        #endregion IApprovalProvider
    }
}
