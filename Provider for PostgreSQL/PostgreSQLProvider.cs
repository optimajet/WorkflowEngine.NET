using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Npgsql;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.PostgreSQL.Models;
using ServiceStack.Text;

namespace OptimaJet.Workflow.PostgreSQL
{
    public class PostgreSQLProvider : IPersistenceProvider, ISchemePersistenceProvider<XElement>, IWorkflowGenerator<XElement>
    {
        public string ConnectionString { get; set; }
        private Core.Runtime.WorkflowRuntime _runtime;
        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public PostgreSQLProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        #region IPersistenceProvider
        public void InitializeProcess(ProcessInstance processInstance)
        {
            using(NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var oldProcess = WorkflowProcessInstance.SelectByKey(connection, processInstance.ProcessId);
                if (oldProcess != null)
                {
                    throw new ProcessAlredyExistsException();
                }
                var newProcess = new WorkflowProcessInstance
                {
                    Id = processInstance.ProcessId,
                    SchemeId = processInstance.SchemeId,
                    ActivityName = processInstance.ProcessScheme.InitialActivity.Name,
                    StateName = processInstance.ProcessScheme.InitialActivity.State
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
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var oldProcess = WorkflowProcessInstance.SelectByKey(connection, processInstance.ProcessId);
                if (oldProcess == null)
                    throw new ProcessNotFoundException();

                oldProcess.SchemeId = processInstance.SchemeId;
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
              processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence).Select(ptp => new { Parameter = ptp, SerializedValue = _runtime.SerializeParameter(ptp.Value, ptp.Type) })
                  .ToList();
            var persistenceParameters = processInstance.ProcessScheme.PersistenceParameters.ToList();

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var persistedParameters = WorkflowProcessInstancePersistence.SelectByProcessId(connection, processInstance.ProcessId).Where(
                        WorkflowProcessInstancep => persistenceParameters.Select(pp => pp.Name).Contains(WorkflowProcessInstancep.ParameterName)).ToList();

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
                                WorkflowProcessInstancePersistence.Delete(connection, persistence.Id);
                        }
                    }
                }
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
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var instanceStatus = WorkflowProcessInstanceStatus.SelectByKey(connection, processInstance.ProcessId);
                if (instanceStatus == null)
                    throw new StatusNotDefinedException();

                if (instanceStatus.Status == ProcessStatus.Running.Id)
                    throw new ImpossibleToSetStatusException();

                instanceStatus.Lock = Guid.NewGuid();
                instanceStatus.Status = ProcessStatus.Running.Id;
                instanceStatus.Update(connection);
            }
        }

        public void SetWorkflowFinalized(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Finalized);
        }

        public void SetWorkflowTerminated(ProcessInstance processInstance, ErrorLevel level, string errorMessage)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Terminated);
        }

        public void ResetWorkflowRunning()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessInstanceStatus.MassChangeStatus(connection, ProcessStatus.Running.Id, ProcessStatus.Idled.Id);
            }
        }

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            var identityId = paramIdentityId == null ? string.Empty : (string)paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string)paramImpIdentityId.Value;

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
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

                    inst.Update(connection);
                }

                var history = new WorkflowProcessTransitionHistory()
                {
                    ActorIdentityId = impIdentityId,
                    ExecutorIdentityId = identityId,
                    Id = Guid.NewGuid(),
                    IsFinalised = false,
                    ProcessId = processInstance.ProcessId,
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
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                return WorkflowProcessInstance.SelectByKey(connection, processId) != null;
            }
        }

        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
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


        private void SetCustomStatus(Guid processId, ProcessStatus status, bool createIfnotDefined = false)
        {
            using(NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var instanceStatus = WorkflowProcessInstanceStatus.SelectByKey(connection, processId);
                if(instanceStatus == null)
                {
                    if(!createIfnotDefined)
                        throw new StatusNotDefinedException();

                    instanceStatus = new WorkflowProcessInstanceStatus()
                    {
                        Id = processId,
                        Lock = Guid.NewGuid(),
                        Status = ProcessStatus.Initialized.Id
                    };
                    instanceStatus.Insert(connection);
                }
                else
                {
                    instanceStatus.Status = ProcessStatus.Initialized.Id;
                    instanceStatus.Update(connection);
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

            List<ParameterDefinitionWithValue> parameters;
            parameters = new List<ParameterDefinitionWithValue>(systemParameters.Count())
            {
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterProcessId.Name),
                    processId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousState.Name),
                    (object) processInstance.PreviousState),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCurrentState.Name),
                    (object) processInstance.StateName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousStateForDirect.Name),
                    (object) processInstance.PreviousStateForDirect),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousStateForReverse.Name),
                    (object) processInstance.PreviousStateForReverse),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivity.Name),
                    (object) processInstance.PreviousActivity),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCurrentActivity.Name),
                    (object) processInstance.ActivityName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivityForDirect.Name),
                    (object) processInstance.PreviousActivityForDirect),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivityForReverse.Name),
                    (object) processInstance.PreviousActivityForReverse),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSchemeCode.Name),
                    (object) processDefinition.Name),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSchemeId.Name),
                    (object) processInstance.SchemeId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterIsPreExecution.Name),
                    false)
            };
            return parameters;
        }

        private IEnumerable<ParameterDefinitionWithValue> GetPersistedProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count());

            List<WorkflowProcessInstancePersistence> persistedParameters;

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                persistedParameters = WorkflowProcessInstancePersistence.SelectByProcessId(connection, processId)
                    .Where(WorkflowProcessInstancep => persistenceParameters.Select(pp => pp.Name).Contains(WorkflowProcessInstancep.ParameterName)).ToList();
            }

            foreach (var persistedParameter in persistedParameters)
            {
                var parameterDefinition = persistenceParameters.Single(p => p.Name == persistedParameter.ParameterName);
                parameters.Add(ParameterDefinition.Create(parameterDefinition, _runtime.DeserializeParameter(persistedParameter.Value, parameterDefinition.Type)));
            }

            return parameters;
        }


        private WorkflowProcessInstance GetProcessInstance(Guid processId)
        {
            using(NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var processInstance = WorkflowProcessInstance.SelectByKey(connection, processId);
                if (processInstance == null)
                    throw new ProcessNotFoundException();
                return processInstance;
            }
        }

        public void DeleteProcess(Guid[] processIds)
        {
            foreach (var processId in processIds)
                DeleteProcess(processId);
        }

        public void DeleteProcess(Guid processId)
        {
            using(NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessInstance.Delete(connection, processId);
                WorkflowProcessInstanceStatus.Delete(connection, processId);
                WorkflowProcessInstancePersistence.DeleteByProcessId(connection, processId);
                WorkflowProcessTransitionHistory.DeleteByProcessId(connection, processId);
                WorkflowProcessTimer.DeleteByProcessId(connection, processId);
            }
        }

        public void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var timer = WorkflowProcessTimer.SelectByProcessIdAndName(connection, processId, name);
                if (timer == null)
                {
                    timer = new WorkflowProcessTimer()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        NextExecutionDateTime = nextExecutionDateTime,
                        ProcessId = processId
                    };

                    timer.Ignore = false;
                    timer.Insert(connection);
                }

                if (!notOverrideIfExists)
                {
                    timer.NextExecutionDateTime = nextExecutionDateTime;
                    timer.Update(connection);
                }
            }
        }

        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessTimer.DeleteByProcessId(connection, processId, timersIgnoreList);
            }
        }

        public void ClearTimersIgnore()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessTimer.ClearTimersIgnore(connection);
            }
        }

        public void ClearTimer(Guid timerId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessTimer.Delete(connection, timerId);
            }
        }

        public DateTime? GetCloseExecutionDateTime()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
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

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var timers = WorkflowProcessTimer.GetTimersToExecute(connection, now);
                WorkflowProcessTimer.SetIgnore(connection, timers);

                return timers.Select(t => new TimerToExecute() { Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id }).ToList();
            }
        }

        public void SaveGlobalParameter<T>(string type, string name, T value)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var parameter = WorkflowGlobalParameter.SelectByTypeAndName(connection, type, name).FirstOrDefault();

                if (parameter == null)
                {
                    parameter = new WorkflowGlobalParameter()
                    {
                        Id = Guid.NewGuid(),
                        Type = type,
                        Name = name,
                        Value = JsonSerializer.SerializeToString(value)
                    };

                    parameter.Insert(connection);
                }

                parameter.Value = JsonSerializer.SerializeToString(value);

                parameter.Update(connection);
            }
        }

        public T LoadGlobalParameter<T>(string type, string name)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var parameter = WorkflowGlobalParameter.SelectByTypeAndName(connection, type, name).FirstOrDefault();

                if (parameter == null)
                    return default(T);

                return JsonSerializer.DeserializeFromString<T>(parameter.Value);
            }

        }

        public List<T> LoadGlobalParameters<T>(string type)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var parameters = WorkflowGlobalParameter.SelectByTypeAndName(connection, type);

                return parameters.Select(p => JsonSerializer.DeserializeFromString<T>(p.Value)).ToList();
            }
        }

        public void DeleteGlobalParameters(string type, string name = null)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowGlobalParameter.DeleteByTypeAndName(connection, type, name);
            }
        }


        #endregion

        #region ISchemePersistenceProvider
        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var processInstance = WorkflowProcessInstance.SelectByKey(connection, processId);
                if (processInstance == null)
                    throw new ProcessNotFoundException();

                if (!processInstance.SchemeId.HasValue)
                    throw new SchemeNotFoundException();

                var schemeDefinition = GetProcessSchemeBySchemeId(processInstance.SchemeId.Value);
                schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
                return schemeDefinition;
            }
        }

        public SchemeDefinition<XElement> GetProcessSchemeBySchemeId(Guid schemeId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessScheme processScheme = WorkflowProcessScheme.SelectByKey(connection, schemeId);

                if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                    throw new SchemeNotFoundException();

                return new SchemeDefinition<XElement>(schemeId, processScheme.SchemeCode, XElement.Parse(processScheme.Scheme), processScheme.IsObsolete);
            }
        }


        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string processName, IDictionary<string, object> parameters)
        {
            return GetProcessSchemeWithParameters(processName, parameters, false);
        }

        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, IDictionary<string, object> parameters, bool ignoreObsolete)
        {
            IEnumerable<WorkflowProcessScheme> processSchemes = new List<WorkflowProcessScheme>();
            var definingParameters = SerializeParameters(parameters);
            var hash = HashHelper.GenerateStringHash(definingParameters);

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                processSchemes =
                WorkflowProcessScheme.Select(connection, schemeCode, hash, ignoreObsolete);
            }

            if (processSchemes.Count() < 1)
                throw new SchemeNotFoundException();

            if (processSchemes.Count() == 1)
            {
                var scheme = processSchemes.First();
                return new SchemeDefinition<XElement>(scheme.Id, scheme.SchemeCode,  XElement.Parse(scheme.Scheme), scheme.IsObsolete);
            }

            foreach (var processScheme in processSchemes.Where(processScheme => processScheme.DefiningParameters == definingParameters))
            {
                return new SchemeDefinition<XElement>(processScheme.Id, processScheme.SchemeCode, XElement.Parse(processScheme.Scheme), processScheme.IsObsolete);
            }

            throw new SchemeNotFoundException();
        }

        public void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = SerializeParameters(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessScheme.SetObsolete(connection, schemeCode, definingParametersHash);
            }
        }

        public void SetSchemeIsObsolete(string schemeCode)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowProcessScheme.SetObsolete(connection, schemeCode);
            }
        }

        public void SaveScheme(string schemeCode, Guid schemeId, XElement scheme, IDictionary<string, object> parameters)
        {
            var definingParameters = SerializeParameters(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                var oldSchemes = WorkflowProcessScheme.Select(connection, schemeCode, definingParametersHash, true);
                if (oldSchemes.Count() > 0)
                {
                    foreach (var oldScheme in oldSchemes)
                        if (oldScheme.DefiningParameters == definingParameters)
                            throw new SchemeAlredyExistsException();
                }

                var newProcessScheme = new WorkflowProcessScheme
                {
                    Id = schemeId,
                    DefiningParameters = definingParameters,
                    DefiningParametersHash = definingParametersHash,
                    Scheme = scheme.ToString(),
                    SchemeCode = schemeCode
                };

                newProcessScheme.Insert(connection);
            }
        }

        public void SaveScheme(string schemaCode, string scheme)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowScheme wfScheme = WorkflowScheme.SelectByKey(connection, schemaCode);
                if (wfScheme == null)
                {
                    wfScheme = new WorkflowScheme();
                    wfScheme.Code = schemaCode;
                    wfScheme.Scheme = scheme;
                    wfScheme.Insert(connection);
                }
                else
                {
                    wfScheme.Scheme = scheme;
                    wfScheme.Update(connection);
                }

            }
        }

        private string SerializeParameters(IDictionary<string, object> parameters)
        {
            var json = new StringBuilder("{");

            bool isFirst = true;

            foreach (var parameter in parameters.OrderBy(p => p.Key))
            {
                if (string.IsNullOrEmpty(parameter.Key))
                    continue;

                if (!isFirst)
                    json.Append(",");

                json.AppendFormat("{0}:[", parameter.Key);

                var isSubFirst = true;

                if (parameter.Value is IEnumerable)
                {
                    var enumerableValue = (parameter.Value as IEnumerable);

                    var valuesToString = new List<string>();

                    foreach (var val in enumerableValue)
                    {
                        valuesToString.Add(val.ToString());
                    }

                    foreach (var parameterValue in valuesToString.OrderBy(p => p))
                    {
                        if (!isSubFirst)
                            json.Append(",");
                        json.AppendFormat("\"{0}\"", parameterValue);
                        isSubFirst = false;
                    }
                }
                else
                {
                    json.AppendFormat("\"{0}\"", parameter.Value);
                }

                json.Append("]");

                isFirst = false;

            }

            json.Append("}");

            return json.ToString();
        }

        public XElement GetScheme(string code)
        {
            using(NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                WorkflowScheme scheme = WorkflowScheme.SelectByKey(connection, code);
                if (scheme == null || string.IsNullOrEmpty(scheme.Scheme))
                    throw new SchemeNotFoundException();

                return XElement.Parse(scheme.Scheme);
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

    }
}
