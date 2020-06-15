using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace OptimaJet.Workflow.DbPersistence
{
    public class MSSQLProvider : IWorkflowProvider, IApprovalProvider
    {
        public string ConnectionString { get; set; }
        private WorkflowRuntime _runtime;
        private readonly bool WriteToHistory;
        private readonly bool WriteSubProcessToRoot;

        public virtual void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public MSSQLProvider(string connectionString, string schema = "dbo", bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            ConnectionString = connectionString;
            DbObject.SchemaName = schema;

            WriteToHistory = writeToHistory;
            WriteSubProcessToRoot = writeSubProcessToRoot;
        }

        #region IPersistenceProvider
        public void DeleteInactiveTimersByProcessId(Guid processId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessTimer.DeleteInactiveByProcessId(connection, processId);
            }
        }

        public async Task DeleteTimerAsync(Guid timerId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await WorkflowProcessTimer.DeleteAsync(connection, timerId).ConfigureAwait(false);
            }
        }

        public List<Guid> GetRunningProcesses(string runtimeId = null)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return WorkflowProcessInstanceStatus.GetProcessesByStatus(connection, ProcessStatus.Running.Id, runtimeId);
            }
        }

        public bool MultiServerRuntimesExist()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return MSSQL.Models.WorkflowRuntime.MultiServerRuntimesExist(connection);
            }
        }

        public int SingleServerRuntimesCount()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return MSSQL.Models.WorkflowRuntime.SingleServerRuntimesCount(connection);
            }
        }

        public int ActiveMultiServerRuntimesCount(string currentRuntimeId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return MSSQL.Models.WorkflowRuntime.ActiveMultiServerRuntimesCount(connection, currentRuntimeId);
            }
        }

        public WorkflowRuntimeModel CreateWorkflowRuntime(string runtimeId, RuntimeStatus status)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var runtime = new MSSQL.Models.WorkflowRuntime()
                {
                    RuntimeId = runtimeId,
                    Lock = Guid.NewGuid(),
                    Status = status
                };

                runtime.Insert(connection);

                return new WorkflowRuntimeModel { Lock = runtime.Lock, RuntimeId = runtimeId, Status = status };
            }
        }

        public void DeleteWorkflowRuntime(string name)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                MSSQL.Models.WorkflowRuntime.Delete(connection, name);
            }
        }

        public WorkflowRuntimeModel UpdateWorkflowRuntimeStatus(WorkflowRuntimeModel runtime, RuntimeStatus status)
        {
            var res = UpdateWorkflowRuntime(runtime, x => x.Status = status, MSSQL.Models.WorkflowRuntime.UpdateStatus);

            if (res.Item1 != 1)
            {
                throw new ImpossibleToSetRuntimeStatusException();
            }

            return res.Item2;
        }

        public  (bool Success, WorkflowRuntimeModel UpdatedModel) UpdateWorkflowRuntimeRestorer(WorkflowRuntimeModel runtime, string restorerId)
        {
            var res = UpdateWorkflowRuntime(runtime, x => x.RestorerId = restorerId, MSSQL.Models.WorkflowRuntime.UpdateRestorer);

            return (res.Item1 == 1, res.Item2);
        }

        public void InitializeProcess(ProcessInstance processInstance)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var oldProcess = WorkflowProcessInstance.SelectByKey(connection, processInstance.ProcessId);
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
                    StartingTransition = processInstance.ProcessScheme.StartingTransition
                };
                newProcess.Insert(connection);
            }
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            BindProcessToNewScheme(processInstance, false);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var oldProcess = WorkflowProcessInstance.SelectByKey(connection, processInstance.ProcessId);
                if (oldProcess == null)
                    throw new ProcessNotFoundException(processInstance.ProcessId);

                oldProcess.SchemeId = processInstance.SchemeId;
                oldProcess.StartingTransition = processInstance.ProcessScheme.StartingTransition;
                if (resetIsDeterminingParametersChanged)
                    oldProcess.IsDeterminingParametersChanged = false;
                oldProcess.Update(connection);
            }
        }

        public void FillProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(GetProcessParameters(processInstance.ProcessId, processInstance.ProcessScheme));
        }

        public void FillPersistedProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(GetPersistedProcessParameters(processInstance.ProcessId, processInstance.ProcessScheme));
        }

        public void FillSystemProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(GetSystemProcessParameters(processInstance.ProcessId, processInstance.ProcessScheme));
        }

        public void SavePersistenceParameters(ProcessInstance processInstance)
        {
            var parametersToPersistList =
                processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence)
                    .Select(ptp =>
                    {
                        if (ptp.Type == typeof(UnknownParameterType))
                            return new {Parameter = ptp, SerializedValue = (string) ptp.Value};
                        return new {Parameter = ptp, SerializedValue = ParametersSerializer.Serialize(ptp.Value, ptp.Type)};
                    })
                    .ToList();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var persistedParameters = WorkflowProcessInstancePersistence.SelectByProcessId(connection, processInstance.ProcessId).ToList();

                foreach (var parameterDefinitionWithValue in parametersToPersistList)
                {
                    var persistence =
                        persistedParameters.SingleOrDefault(
                            pp => pp.ParameterName == parameterDefinitionWithValue.Parameter.Name);
                    {
                        if (persistence == null)
                        {
                            if (parameterDefinitionWithValue.SerializedValue != null)
                            {
                                persistence = new WorkflowProcessInstancePersistence()
                                {
                                    Id = Guid.NewGuid(),
                                    ProcessId = processInstance.ProcessId,
                                    ParameterName = parameterDefinitionWithValue.Parameter.Name,
                                    Value = parameterDefinitionWithValue.SerializedValue
                                };
                                persistence.Insert(connection);
                            }
                        }
                        else
                        {
                            if (parameterDefinitionWithValue.SerializedValue != null)
                            {
                                persistence.Value = parameterDefinitionWithValue.SerializedValue;
                                persistence.Update(connection);
                            }
                            else
                            {
                                WorkflowProcessInstancePersistence.Delete(connection, persistence.Id);
                            }
                        }
                    }
                }
            }
        }
        
        public void SetProcessStatus(Guid processId, ProcessStatus newStatus)
        {
            if (newStatus == ProcessStatus.Running)
            {
                SetRunningStatus(processId);
            }
            else
            {
                SetCustomStatus(processId,newStatus);
            }
        }

        public void SetWorkflowIniialized(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Initialized, true);
        }

        public void SetWorkflowIdled(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Idled);
        }

        public void SetWorkflowRunning(ProcessInstance processInstance)
        {
            var processId = processInstance.ProcessId;
            SetRunningStatus(processId);
        }

        public void SetWorkflowFinalized(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Finalized);
        }

#pragma warning disable 612
        public void SetWorkflowTerminated(ProcessInstance processInstance)
#pragma warning restore 612
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Terminated);
        }

        public void ResetWorkflowRunning()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessInstanceStatus.MassChangeStatus(connection, ProcessStatus.Running.Id, ProcessStatus.Idled.Id, 
                    _runtime.RuntimeDateTimeNow);
            }
        }

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            var identityId = paramIdentityId == null ? string.Empty : (string)paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string)paramImpIdentityId.Value;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessInstance inst = WorkflowProcessInstance.SelectByKey(connection, processInstance.ProcessId);

                if (inst != null)
                {
                    if (!string.IsNullOrEmpty(transition.To.State))
                        inst.StateName = transition.To.State;

                    inst.ActivityName = transition.To.Name;
                    inst.PreviousActivity = transition.From.Name;

                    if (!string.IsNullOrEmpty(transition.From.State))
                        inst.PreviousState = transition.From.State;

                    if (transition.Classifier == TransitionClassifier.Direct)
                    {
                        inst.PreviousActivityForDirect = transition.From.Name;

                        if (!string.IsNullOrEmpty(transition.From.State))
                            inst.PreviousStateForDirect = transition.From.State;
                    }
                    else if (transition.Classifier == TransitionClassifier.Reverse)
                    {
                        inst.PreviousActivityForReverse = transition.From.Name;

                        if (!string.IsNullOrEmpty(transition.From.State))
                            inst.PreviousStateForReverse = transition.From.State;
                    }

                    inst.ParentProcessId = processInstance.ParentProcessId;
                    inst.RootProcessId = processInstance.RootProcessId;

                    inst.Update(connection);
                }

                if (!WriteToHistory)
                    return;

                var history = new WorkflowProcessTransitionHistory()
                {
                    ActorIdentityId = impIdentityId,
                    ExecutorIdentityId = identityId,
                    Id = Guid.NewGuid(),
                    IsFinalised = transition.To.IsFinal,
                    ProcessId = (WriteSubProcessToRoot && processInstance.IsSubprocess) ? processInstance.RootProcessId : processInstance.ProcessId,
                    FromActivityName = transition.From.Name,
                    FromStateName = transition.From.State,
                    ToActivityName = transition.To.Name,
                    ToStateName = transition.To.State,
                    TransitionClassifier =
                        transition.Classifier.ToString(),
                    TransitionTime = _runtime.RuntimeDateTimeNow,
                    TriggerName = string.IsNullOrEmpty(processInstance.ExecutedTimer) ? processInstance.CurrentCommand : processInstance.ExecutedTimer
                };
                history.Insert(connection);
            }
        }

        public bool IsProcessExists(Guid processId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                return WorkflowProcessInstance.SelectByKey(connection, processId) != null;
            }
        }

        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var instance = WorkflowProcessInstanceStatus.SelectByKey(connection, processId);
                if (instance == null)
                    return ProcessStatus.NotFound;
                var status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status);
                if (status == null)
                    return ProcessStatus.Unknown;
                return status;
            }
        }
        
        private void SetRunningStatus(Guid processId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var instanceStatus = WorkflowProcessInstanceStatus.SelectByKey(connection, processId);

                if (instanceStatus == null)
                    throw new StatusNotDefinedException();

                if (instanceStatus.Status == ProcessStatus.Running.Id)
                {
                    throw new ImpossibleToSetStatusException("Process already running");
                }

                var oldLock = instanceStatus.Lock;

                instanceStatus.Lock = Guid.NewGuid();
                instanceStatus.Status = ProcessStatus.Running.Id;
                instanceStatus.RuntimeId = _runtime.Id;
                instanceStatus.SetTime = _runtime.RuntimeDateTimeNow;

                int cnt = WorkflowProcessInstanceStatus.ChangeStatus(connection, instanceStatus, oldLock);

                if (cnt != 1)
                    throw new ImpossibleToSetStatusException();
            }
        }

        private void SetCustomStatus(Guid processId, ProcessStatus status, bool createIfNotDefined = false)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var instanceStatus = WorkflowProcessInstanceStatus.SelectByKey(connection, processId);
                if (instanceStatus == null)
                {
                    if (!createIfNotDefined)
                        throw new StatusNotDefinedException();

                    instanceStatus = new WorkflowProcessInstanceStatus()
                    {
                        Id = processId,
                        Lock = Guid.NewGuid(),
                        Status = status.Id,
                        RuntimeId = _runtime.Id,
                        SetTime = _runtime.RuntimeDateTimeNow
                    };

                    instanceStatus.Insert(connection);
                }
                else
                {
                    var oldLock = instanceStatus.Lock;

                    instanceStatus.Status = status.Id;
                    instanceStatus.Lock = Guid.NewGuid();
                    instanceStatus.RuntimeId = _runtime.Id;
                    instanceStatus.SetTime = _runtime.RuntimeDateTimeNow;

                    var cnt = WorkflowProcessInstanceStatus.ChangeStatus(connection, instanceStatus, oldLock);

                    if (cnt != 1)
                        throw new ImpossibleToSetStatusException();
                }

            }
        }

        private IEnumerable<ParameterDefinitionWithValue> GetProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            var parameters = new List<ParameterDefinitionWithValue>(processDefinition.Parameters.Count());
            parameters.AddRange(GetPersistedProcessParameters(processId, processDefinition));
            parameters.AddRange(GetSystemProcessParameters(processId, processDefinition));
            return parameters;
        }

        private IEnumerable<ParameterDefinitionWithValue> GetSystemProcessParameters(Guid processId,
            ProcessDefinition processDefinition)
        {
            var processInstance = GetProcessInstance(processId);

            var systemParameters =
                processDefinition.Parameters.Where(p => p.Purpose == ParameterPurpose.System).ToList();

            var parameters = new List<ParameterDefinitionWithValue>(systemParameters.Count())
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
                    processInstance.TenantId)
            };
            return parameters;
        }

        private IEnumerable<ParameterDefinitionWithValue> GetPersistedProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count());

            List<WorkflowProcessInstancePersistence> persistedParameters;

            using (var connection = new SqlConnection(ConnectionString))
            {
                persistedParameters = WorkflowProcessInstancePersistence.SelectByProcessId(connection, processId).ToList();
            }

            foreach (var persistedParameter in persistedParameters)
            {
                var parameterDefinition = persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.ParameterName);
                if (parameterDefinition == null)
                {
                    parameterDefinition = ParameterDefinition.Create(persistedParameter.ParameterName, typeof(UnknownParameterType), ParameterPurpose.Persistence);
                    parameters.Add(ParameterDefinition.Create(parameterDefinition, persistedParameter.Value));
                }
                else
                {
                    parameters.Add(ParameterDefinition.Create(parameterDefinition, ParametersSerializer.Deserialize(persistedParameter.Value, parameterDefinition.Type)));
                }
            }

            return parameters;
        }


        private WorkflowProcessInstance GetProcessInstance(Guid processId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var processInstance = WorkflowProcessInstance.SelectByKey(connection, processId);
                if (processInstance == null)
                    throw new ProcessNotFoundException(processId);
                return processInstance;
            }
        }

        public void DeleteProcess(Guid[] processIds)
        {
            foreach (var processId in processIds)
            {
                DeleteProcess(processId);
            }
        }

        public void SaveGlobalParameter<T>(string type, string name, T value)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var parameter = WorkflowGlobalParameter.SelectByTypeAndName(connection, type, name).FirstOrDefault();

                if (parameter == null)
                {
                    parameter = new WorkflowGlobalParameter()
                    {
                        Id = Guid.NewGuid(),
                        Type = type,
                        Name = name,
                        Value = JsonConvert.SerializeObject(value)
                    };

                    parameter.Insert(connection);
                }
                else
                {
                    parameter.Value = JsonConvert.SerializeObject(value);

                    parameter.Update(connection);
                }
            }
        }

        public T LoadGlobalParameter<T>(string type, string name)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var parameter = WorkflowGlobalParameter.SelectByTypeAndName(connection, type, name).FirstOrDefault();

                if (parameter == null)
                    return default(T);

                return JsonConvert.DeserializeObject<T>(parameter.Value);
            }

        }

        public List<T> LoadGlobalParameters<T>(string type)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var parameters = WorkflowGlobalParameter.SelectByTypeAndName(connection, type);

                return parameters.Select(p => JsonConvert.DeserializeObject<T>(p.Value)).ToList();
            }
        }

        public void DeleteGlobalParameters(string type, string name = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowGlobalParameter.DeleteByTypeAndName(connection, type, name);
            }
        }

        public void DeleteProcess(Guid processId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    WorkflowProcessInstance.Delete(connection, processId, transaction);
                    WorkflowProcessInstanceStatus.Delete(connection, processId, transaction);
                    WorkflowProcessInstancePersistence.DeleteByProcessId(connection, processId, transaction);
                    WorkflowProcessTransitionHistory.DeleteByProcessId(connection, processId, transaction);
                    WorkflowProcessTimer.DeleteByProcessId(connection, processId, null, transaction);
                    transaction.Commit();
                }

            }
        }

        public void RegisterTimer(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var timer = WorkflowProcessTimer.SelectByProcessIdAndName(connection, processId, name);
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

                    timer.Insert(connection);
                }
                else if (!notOverrideIfExists)
                {
                    timer.NextExecutionDateTime = nextExecutionDateTime;
                    timer.Update(connection);
                }
            }
        }

        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessTimer.DeleteByProcessId(connection, processId, timersIgnoreList);
            }
        }
        public void ClearTimerIgnore(Guid timerId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessTimer.ClearTimerIgnore(connection, timerId);
            }
        }

        public int SetTimerIgnore(Guid timerId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return WorkflowProcessTimer.SetTimerIgnore(connection, timerId);
            }
        }

        public void ClearTimer(Guid timerId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessTimer.Delete(connection, timerId);
            }
        }

        public DateTime? GetCloseExecutionDateTime()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var timer = WorkflowProcessTimer.GetCloseExecutionTimer(connection);
                if (timer == null)
                    return null;

                return timer.NextExecutionDateTime;
            }
        }

        public List<TimerToExecute> GetTimersToExecute()
        {
            var now = _runtime.RuntimeDateTimeNow;

            using (var connection = new SqlConnection(ConnectionString))
            {
                var timers = WorkflowProcessTimer.GetTimersToExecute(connection, now);
                WorkflowProcessTimer.SetIgnore(connection, timers);

                return timers.Select(t => new TimerToExecute() {Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id}).ToList();
            }
        }

        public List<Core.Model.WorkflowTimer> GetTopTimersToExecute(int top)
        {
            DateTime now = _runtime.RuntimeDateTimeNow;

            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessTimer[] timers = WorkflowProcessTimer.GetTopTimersToExecute(connection, top, now);

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
        }

        public List<ProcessHistoryItem> GetProcessHistory(Guid processId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return WorkflowProcessTransitionHistory.SelectByProcessId(connection, processId)
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
                        TransitionClassifier = (TransitionClassifier) Enum.Parse(typeof(TransitionClassifier), hi.TransitionClassifier),
                        TransitionTime = hi.TransitionTime,
                        TriggerName = hi.TriggerName
                    })
                    .ToList();
            }
        }

        public IEnumerable<ProcessTimer> GetTimersForProcess(Guid processId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var timers = WorkflowProcessTimer.SelectByProcessId(connection, processId);
                return timers.Select(t => new ProcessTimer(t.Id, t.Name, t.NextExecutionDateTime));
            }
        }

        public async Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return await ProcessInstanceTreeItem.GetProcessTreeItemsByRootProcessId(connection, rootProcessId).ConfigureAwait(false);
            }
        }

        public IEnumerable<ProcessTimer> GetActiveTimersForProcess(Guid processId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var timers = WorkflowProcessTimer.SelectActiveByProcessId(connection, processId);
                return timers.Select(t => new ProcessTimer(t.Id, t.Name, t.NextExecutionDateTime));
            }
        }

        public WorkflowRuntimeModel GetWorkflowRuntimeModel(string runtimeId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return MSSQL.Models.WorkflowRuntime.GetWorkflowRuntimeStatus(connection, runtimeId);
            }
        }

        public int SendRuntimeLastAliveSignal()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return MSSQL.Models.WorkflowRuntime.SendRuntimeLastAliveSignal(connection, _runtime.Id, _runtime.RuntimeDateTimeNow);
            }
        }

        public DateTime? GetNextTimerDate(TimerCategory timerCategory, int timerInterval)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                string categoryName = timerCategory.ToString();

                var syncLock = MSSQL.Models.WorkflowSync.GetByName(connection, categoryName);

                if (syncLock == null)
                {
                    throw new Exception($"Sync lock {categoryName} not found");
                }

                string nextTimeColumnName = null;

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

                DateTime? max = MSSQL.Models.WorkflowRuntime.GetMaxNextTime(connection, _runtime.Id, nextTimeColumnName);

                DateTime result = _runtime.RuntimeDateTimeNow;

                if (max > result)
                {
                    result = max.Value;
                }
                
                result += TimeSpan.FromMilliseconds(timerInterval);
                
                var newLock = Guid.NewGuid();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    MSSQL.Models.WorkflowRuntime.UpdateNextTime(connection, _runtime.Id, nextTimeColumnName, result, transaction);
                   
                    var rowCount = MSSQL.Models.WorkflowSync.UpdateLock(connection, categoryName, syncLock.Lock, newLock, transaction);

                    if (rowCount == 0)
                    {
                        transaction.Rollback();
                        return null;
                    }
                    
                    transaction.Commit();
                }

                return result;
            }
        }

        public List<WorkflowRuntimeModel> GetWorkflowRuntimes()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return MSSQL.Models.WorkflowRuntime.SelectAll(connection).Select(GetModel).ToList();
            }
        }


        private WorkflowRuntimeModel GetModel(MSSQL.Models.WorkflowRuntime result)
        {
            return new WorkflowRuntimeModel { Lock = result.Lock, RuntimeId = result.RuntimeId, Status = result.Status, RestorerId = result.RestorerId, LastAliveSignal = result.LastAliveSignal, NextTimerTime = result.NextTimerTime };
        }

        public IApprovalProvider GetIApprovalProvider()
        {
            return this;
        }

        #endregion

        #region ISchemePersistenceProvider

        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var processInstance = WorkflowProcessInstance.SelectByKey(connection, processId);
                if (processInstance == null)
                    throw new ProcessNotFoundException(processId);

                if (!processInstance.SchemeId.HasValue)
                    throw SchemeNotFoundException.Create(processId, SchemeLocation.WorkflowProcessInstance);

                var schemeDefinition = GetProcessSchemeBySchemeId(processInstance.SchemeId.Value);
                schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
                return schemeDefinition;
            }
        }

        public SchemeDefinition<XElement> GetProcessSchemeBySchemeId(Guid schemeId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var processScheme = WorkflowProcessScheme.SelectByKey(connection, schemeId);

                if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                    throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

                return ConvertToSchemeDefinition(processScheme);
            }
        }

        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, string definingParameters,
            Guid? rootSchemeId, bool ignoreObsolete)
        {
            IEnumerable<WorkflowProcessScheme> processSchemes;
            var hash = HashHelper.GenerateStringHash(definingParameters);

            using (var connection = new SqlConnection(ConnectionString))
            {
                processSchemes =
                    WorkflowProcessScheme.Select(connection, schemeCode, hash, ignoreObsolete ? false : (bool?) null,
                        rootSchemeId);
            }

            if (!processSchemes.Any())
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme, definingParameters);

            if (processSchemes.Count() == 1)
            {
                var scheme = processSchemes.First();
                return ConvertToSchemeDefinition(scheme);
            }

            foreach (var processScheme in processSchemes.Where(processScheme => processScheme.DefiningParameters == definingParameters))
            {
                return ConvertToSchemeDefinition(processScheme);
            }

            throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme, definingParameters);
        }

        public void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessScheme.SetObsolete(connection, schemeCode, definingParametersHash);
            }
        }

        public void SetSchemeIsObsolete(string schemeCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowProcessScheme.SetObsolete(connection, schemeCode);
            }
        }

        public SchemeDefinition<XElement> SaveScheme(SchemeDefinition<XElement> scheme)
        {
            var definingParameters = scheme.DefiningParameters;
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);


            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var oldSchemes = WorkflowProcessScheme.Select(connection, scheme.SchemeCode, definingParametersHash,
                    scheme.IsObsolete, scheme.RootSchemeId);

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

                newProcessScheme.Insert(connection);

                return ConvertToSchemeDefinition(newProcessScheme);
            }
        }

        public virtual void SaveScheme(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme,
            List<string> tags)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowScheme wfScheme = WorkflowScheme.SelectByKey(connection, schemaCode);
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
                    wfScheme.Insert(connection);
                }
                else
                {
                    wfScheme.Scheme = scheme;
                    wfScheme.CanBeInlined = canBeInlined;
                    wfScheme.InlinedSchemes = inlinedSchemes.Any() ? JsonConvert.SerializeObject(inlinedSchemes) : null;
                    wfScheme.Tags = TagHelper.ToTagStringForDatabase(tags);
                    wfScheme.Update(connection);
                }

            }
        }



        public virtual XElement GetScheme(string code)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowScheme scheme = WorkflowScheme.SelectByKey(connection, code);
                if (scheme == null || string.IsNullOrEmpty(scheme.Scheme))
                    throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowProcessScheme);

                return XElement.Parse(scheme.Scheme);
            }
        }

        public virtual List<string> GetInlinedSchemeCodes()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                return WorkflowScheme.GetInlinedSchemeCodes(connection);
            }
        }

        public virtual List<string> GetRelatedByInliningSchemeCodes(string schemeCode)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                return WorkflowScheme.GetRelatedSchemeCodes(connection,schemeCode);
            }
        }

        public List<string> SearchSchemesByTags(params string[] tags)
        {
            return SearchSchemesByTags(tags?.AsEnumerable());
        }

        public virtual List<string> SearchSchemesByTags(IEnumerable<string> tags)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return WorkflowScheme.GetSchemeCodesByTags(connection, tags);
            }
        }

        public void AddSchemeTags(string schemeCode, params string[] tags)
        {
            AddSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public virtual void AddSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowScheme.AddSchemeTags(connection, schemeCode, tags, _runtime.Builder);
            }
        }

        public void RemoveSchemeTags(string schemeCode, params string[] tags)
        {
            RemoveSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public virtual void RemoveSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowScheme.RemoveSchemeTags(connection, schemeCode, tags, _runtime.Builder);
            }
        }

        public void SetSchemeTags(string schemeCode, params string[] tags)
        {
            SetSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public virtual void SetSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                WorkflowScheme.SetSchemeTags(connection, schemeCode, tags, _runtime.Builder);
            }
        }

        #endregion

        #region IWorkflowGenerator

        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
                throw new InvalidOperationException("Parameters not supported");

            return GetScheme(schemeCode);
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

        private Tuple<int,WorkflowRuntimeModel> UpdateWorkflowRuntime(WorkflowRuntimeModel runtime, Action<WorkflowRuntimeModel> setter, 
            Func<SqlConnection, WorkflowRuntimeModel, Guid, int> updateMethod)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                Guid oldLock = runtime.Lock;
                setter(runtime);
                runtime.Lock = Guid.NewGuid();

                int cnt = updateMethod(connection, runtime, oldLock);
                
                if (cnt != 1)
                {
                    return new Tuple<int, WorkflowRuntimeModel>(cnt,null);
                }
                
                return new Tuple<int, WorkflowRuntimeModel>(cnt,runtime);
            }
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

        public async Task BulkInitProcesses(List<ProcessInstance> instances, ProcessStatus status, CancellationToken token)
        {
            await BulkInitProcesses(instances, null, status, token).ConfigureAwait(false);
        }

        public async Task BulkInitProcesses(List<ProcessInstance> instances, List<TimerToRegister> timers, ProcessStatus status, CancellationToken token)
        {
#if NETCOREAPP && !NETCORE2
            throw new NotImplementedException();
#else
            if (token.IsCancellationRequested)
                return;

            var needRegisterTimers = timers != null && timers.Any();

            var piDataTable = WorkflowProcessInstance.ToDataTable();
            var psDataTable = WorkflowProcessInstanceStatus.ToDataTable();
            var ppDataTable = WorkflowProcessInstancePersistence.ToDataTable();
            var ptDataTable = WorkflowProcessTimer.ToDataTable();

            foreach (var processInstance in instances)
            {
                var processRow = piDataTable.NewRow();
                processRow["Id"] = processInstance.ProcessId;
                processRow["SchemeId"] = processInstance.SchemeId;
                processRow["ActivityName"] = processInstance.ProcessScheme.InitialActivity.Name;
                processRow["StateName"] = processInstance.ProcessScheme.InitialActivity.State;
                processRow["RootProcessId"] = processInstance.RootProcessId;
                processRow["TenantId"] = processInstance.TenantId;
                if (processInstance.ParentProcessId.HasValue)
                    processRow["ParentProcessId"] = processInstance.ParentProcessId;
                piDataTable.Rows.Add(processRow);
                var statusRow = psDataTable.NewRow();
                statusRow["Id"] = processInstance.ProcessId;
                statusRow["Lock"] = Guid.NewGuid();
                statusRow["Status"] = status.Id;
                statusRow["RuntimeId"] = _runtime.Id;
                statusRow["SetTime"] = _runtime.RuntimeDateTimeNow;
                psDataTable.Rows.Add(statusRow);

                var parametersToPesist = processInstance.ProcessParameters.Where(p => p.Purpose == ParameterPurpose.Persistence && p.Value != null).ToList();

                foreach (var parameter in parametersToPesist)
                {
                    var parameterRow = ppDataTable.NewRow();
                    parameterRow["Id"] = Guid.NewGuid();
                    parameterRow["ProcessId"] = processInstance.ProcessId;
                    parameterRow["ParameterName"] = parameter.Name;
                    parameterRow["Value"] = ParametersSerializer.Serialize(parameter.Value, parameter.Type);
                    ppDataTable.Rows.Add(parameterRow);
                }
            }

            if (needRegisterTimers)
            {
                foreach (var timer in timers)
                {
                    var timerRow = ptDataTable.NewRow();
                    timerRow["Id"] = Guid.NewGuid();
                    timerRow["ProcessId"] = timer.ProcessId;
                    timerRow["RootProcessId"] = timer.ProcessId;
                    timerRow["Name"] = timer.Name;
                    timerRow["NextExecutionDateTime"] = timer.ExecutionDateTime;
                    timerRow["Ignore"] = false;
                    ptDataTable.Rows.Add(timerRow);
                }
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
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
                }
            }
#endif
        }

        #endregion

        #region IApprovalProvider

        public async Task DropWorkflowInboxAsync(Guid processId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                WorkflowInbox.DeleteByProcessId(connection, processId);
            }
        }

        public async Task InsertInboxAsync(Guid processId, List<string> newActors)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var inboxItems = newActors.Select(newactor => new WorkflowInbox() { Id = Guid.NewGuid(), IdentityId = newactor, ProcessId = processId }).ToArray();
                await WorkflowInbox.InsertAllAsync(connection, inboxItems).ConfigureAwait(false);
            }
        }

        public async Task WriteApprovalHistoryAsync(Guid id, string currentState, string nextState, string triggerName, string allowedToEmployeeNames, long order)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var historyItem = new WorkflowApprovalHistory
                {
                    Id = Guid.NewGuid(),
                    AllowedTo = allowedToEmployeeNames,
                    DestinationState = nextState,
                    ProcessId = id,
                    InitialState = currentState,
                    TriggerName = triggerName,
                    Sort = order
                };

                await historyItem.InsertAsync(connection).ConfigureAwait(false);
            }
        }

        public async Task UpdateApprovalHistoryAsync(Guid id, string currentState, string nextState, string triggerName, string identityId, long order, string comment)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var historyItem = WorkflowApprovalHistory.SelectByProcessId(connection, id).FirstOrDefault(h =>
                    h.ProcessId == id && !h.TransitionTime.HasValue &&
                    h.InitialState == currentState && h.DestinationState == nextState);

                if (historyItem == null)
                {
                    historyItem = new WorkflowApprovalHistory
                    {
                        Id = Guid.NewGuid(),
                        AllowedTo = string.Empty,
                        DestinationState = nextState,
                        ProcessId = id,
                        InitialState = currentState,
                        Sort = order,
                        TriggerName = triggerName,
                        Commentary = comment,
                        TransitionTime = _runtime.RuntimeDateTimeNow,
                        IdentityId = identityId
                    };

                    await historyItem.InsertAsync(connection).ConfigureAwait(false);
                }
                else
                {
                    historyItem.TriggerName = triggerName;
                    historyItem.TransitionTime = _runtime.RuntimeDateTimeNow;
                    historyItem.IdentityId = identityId;
                    historyItem.Commentary = comment;
                    await historyItem.UpdateAsync(connection).ConfigureAwait(false);
                }
            }
        }

        public async Task DropEmptyApprovalHistoryAsync(Guid processId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                foreach (var record in WorkflowApprovalHistory.SelectByProcessId(connection, processId).Where(x => !x.TransitionTime.HasValue).ToList())
                {
                    await WorkflowApprovalHistory.DeleteAsync(connection, record.Id).ConfigureAwait(false);
                }
            }
        }

        #endregion IApprovalProvider
    }
}
