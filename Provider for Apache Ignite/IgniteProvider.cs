using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Transactions;
using Apache.Ignite.Linq;

namespace OptimaJet.Workflow.Ignite
{
    public class IgniteConstants
    {
        public const string WorkflowProcessInstanceCacheName = "WorkflowProcessInstance";
        public const string WorkflowProcessInstanceStatusCacheName = "WorkflowProcessInstanceStatus";
        public const string WorkflowProcessSchemeCacheName = "WorkflowProcessScheme";
        public const string WorkflowProcessTransitionHistoryCacheName = "WorkflowProcessTransitionHistory";
        public const string WorkflowSchemeCacheName = "WorkflowScheme";
        public const string WorkflowProcessTimerCacheName = "WorkflowProcessTimer";
        public const string WorkflowGlobalParameterCacheName = "WorkflowGlobalParameter";
    }

    public class IgniteProvider : IPersistenceProvider, ISchemePersistenceProvider<XElement>,
        IWorkflowGenerator<XElement>
    {
        private WorkflowRuntime _runtime;

        public IgniteProvider(IIgnite store)
        {
            Store = store;
        }

        public IIgnite Store { get; set; }

        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        private SchemeDefinition<XElement> ConvertToSchemeDefinition(WorkflowProcessScheme workflowProcessScheme)
        {
            return new SchemeDefinition<XElement>(workflowProcessScheme.Id,
                !string.IsNullOrEmpty(workflowProcessScheme.RootSchemeId)
                    ? (Guid?) Guid.Parse(workflowProcessScheme.RootSchemeId)
                    : null,
                workflowProcessScheme.SchemeCode, workflowProcessScheme.RootSchemeCode,
                XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete, false,
                JsonConvert.DeserializeObject<List<string>>(workflowProcessScheme.AllowedActivities ?? "null"),
                workflowProcessScheme.StartingTransition,
                workflowProcessScheme.DefiningParameters);
        }

        #region IPersistenceProvider

        public void InitializeProcess(ProcessInstance processInstance)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);

            if (cache.ContainsKey(processInstance.ProcessId))
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
                Persistence = new List<WorkflowProcessInstancePersistence>()
            };
            cache.Put(newProcess.Id, newProcess);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            BindProcessToNewScheme(processInstance, false);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);

            WorkflowProcessInstance oldProcess = null;
            try
            {
                oldProcess = cache.Get(processInstance.ProcessId);
            }
            catch (KeyNotFoundException)
            {
            }

            if (oldProcess == null)
                throw new ProcessNotFoundException(processInstance.ProcessId);

            oldProcess.SchemeId = processInstance.SchemeId;
            if (resetIsDeterminingParametersChanged)
                oldProcess.IsDeterminingParametersChanged = false;
            cache.Put(oldProcess.Id, oldProcess);
        }

        public void FillProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(GetProcessParameters(processInstance.ProcessId, processInstance.ProcessScheme));
        }

        public void FillPersistedProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(GetPersistedProcessParameters(processInstance.ProcessId,
                processInstance.ProcessScheme));
        }

        public void FillSystemProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(GetSystemProcessParameters(processInstance.ProcessId,
                processInstance.ProcessScheme));
        }

        public void SavePersistenceParameters(ProcessInstance processInstance)
        {
            var parametersToPersistList =
                processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence)
                    .Select(
                        ptp => new {Parameter = ptp, SerializedValue = _runtime.SerializeParameter(ptp.Value, ptp.Type)})
                    .ToList();

            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);
            var process = cache.Get(processInstance.ProcessId);

            if (process != null && process.Persistence != null)
            {
                var persistedParameters = process.Persistence.ToList();

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
                                persistence = new WorkflowProcessInstancePersistence
                                {
                                    ParameterName = parameterDefinitionWithValue.Parameter.Name,
                                    Value = parameterDefinitionWithValue.SerializedValue
                                };
                                process.Persistence.Add(persistence);
                            }
                        }
                        else
                        {
                            if (parameterDefinitionWithValue.SerializedValue != null)
                                persistence.Value = parameterDefinitionWithValue.SerializedValue;
                            else
                                process.Persistence.Remove(persistence);
                        }
                    }
                }

                cache.Put(process.Id, process);
            }
        }

        public void SetWorkflowIniialized(ProcessInstance processInstance)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstanceStatus>(
                    IgniteConstants.WorkflowProcessInstanceStatusCacheName);
            WorkflowProcessInstanceStatus instanceStatus = null;
            if (!cache.ContainsKey(processInstance.ProcessId))
            {
                instanceStatus = new WorkflowProcessInstanceStatus
                {
                    Id = processInstance.ProcessId,
                    Status = ProcessStatus.Initialized.Id
                };
            }
            else
            {
                instanceStatus = cache.Get(processInstance.ProcessId);
                instanceStatus.Status = ProcessStatus.Initialized.Id;
            }

            cache.Put(instanceStatus.Id, instanceStatus);
        }

        public void SetWorkflowIdled(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Idled);
        }

        public void SetWorkflowRunning(ProcessInstance processInstance)
        {
            using (var tx = Store.GetTransactions().TxStart())
            {
                var cache =
                    Store.GetOrCreateCache<Guid, WorkflowProcessInstanceStatus>(
                        IgniteConstants.WorkflowProcessInstanceStatusCacheName);
                WorkflowProcessInstanceStatus instanceStatus = null;
                try
                {
                    instanceStatus = cache.Get(processInstance.ProcessId);
                }
                catch (KeyNotFoundException)
                {
                }

                if (instanceStatus == null)
                    throw new StatusNotDefinedException();

                if (instanceStatus.Status == ProcessStatus.Running.Id)
                    throw new ImpossibleToSetStatusException();

                instanceStatus.Status = ProcessStatus.Running.Id;
                cache.Put(instanceStatus.Id, instanceStatus);

                tx.Commit();
            }
        }

        public void SetWorkflowFinalized(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Finalized);
        }

#pragma warning disable 612
        public void SetWorkflowTerminated(ProcessInstance processInstance, ErrorLevel level, string errorMessage)
#pragma warning restore 612
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Terminated);
        }

        public void ResetWorkflowRunning()
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstanceStatus>(
                    IgniteConstants.WorkflowProcessInstanceStatusCacheName);
            var runningId = ProcessStatus.Running.Id;
            var status = cache.AsCacheQueryable().Where(c => c.Value.Status == runningId).ToList();

            foreach (var s in status)
            {
                s.Value.Status = ProcessStatus.Idled.Id;
            }

            cache.PutAll(status.ToDictionary(c => c.Key, c => c.Value));
        }

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId =
                processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            var identityId = paramIdentityId == null ? string.Empty : (string) paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string) paramImpIdentityId.Value;

            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);
            var inst = cache.Get(processInstance.ProcessId);
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

                cache.Put(inst.Id, inst);
            }

            var history = new WorkflowProcessTransitionHistory
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
                TriggerName =
                    string.IsNullOrEmpty(processInstance.ExecutedTimer)
                        ? processInstance.CurrentCommand
                        : processInstance.ExecutedTimer
            };

            var cacheTransition =
                Store.GetOrCreateCache<Guid, WorkflowProcessTransitionHistory>(
                    IgniteConstants.WorkflowProcessTransitionHistoryCacheName);
            cacheTransition.Put(history.Id, history);
        }

        public bool IsProcessExists(Guid processId)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);
            return cache.ContainsKey(processId);
        }

        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstanceStatus>(
                    IgniteConstants.WorkflowProcessInstanceStatusCacheName);

            WorkflowProcessInstanceStatus instance = null;
            try
            {
                instance = cache.Get(processId);
            }
            catch (KeyNotFoundException)
            {
            }

            if (instance == null)
                return ProcessStatus.NotFound;
            var status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status);
            if (status == null)
                return ProcessStatus.Unknown;
            return status;
        }


        private void SetCustomStatus(Guid processId, ProcessStatus status)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstanceStatus>(
                    IgniteConstants.WorkflowProcessInstanceStatusCacheName);

            if (!cache.ContainsKey(processId))
                throw new StatusNotDefinedException();

            var instanceStatus = cache.Get(processId);
            if (instanceStatus == null)
                throw new StatusNotDefinedException();
            instanceStatus.Status = status.Id;

            cache.Put(instanceStatus.Id, instanceStatus);
        }

        private IEnumerable<ParameterDefinitionWithValue> GetProcessParameters(Guid processId,
            ProcessDefinition processDefinition)
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
                    processInstance.RootProcessId)
            };
            return parameters;
        }

        private IEnumerable<ParameterDefinitionWithValue> GetPersistedProcessParameters(Guid processId,
            ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count());

            List<WorkflowProcessInstancePersistence> persistedParameters;
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);
            var process = cache.Get(processId);
            if (process != null && process.Persistence != null)
            {
                persistedParameters = process.Persistence.ToList();
            }
            else
            {
                return parameters;
                //persistedParameters = new List<WorkflowProcessInstancePersistence>();
            }

            foreach (var persistedParameter in persistedParameters)
            {
                var parameterDefinition =
                    persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.ParameterName) ??
                    ParameterDefinition.Create(persistedParameter.ParameterName, "System.String",
                        ParameterPurpose.Persistence.ToString(), null);
                parameters.Add(ParameterDefinition.Create(parameterDefinition,
                    _runtime.DeserializeParameter(persistedParameter.Value, parameterDefinition.Type)));
            }

            return parameters;
        }


        private WorkflowProcessInstance GetProcessInstance(Guid processId)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);

            WorkflowProcessInstance processInstance = null;
            try
            {
                processInstance = cache.Get(processId);
            }
            catch (KeyNotFoundException)
            {
            }

            if (processInstance == null)
                throw new ProcessNotFoundException(processId);
            return processInstance;
        }

        public void DeleteProcess(Guid[] processIds)
        {
            foreach (var processId in processIds)
                DeleteProcess(processId);
        }

        public void SaveGlobalParameter<T>(string type, string name, T value)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowGlobalParameter>(IgniteConstants.WorkflowGlobalParameterCacheName);

            var parameter =
                cache.AsCacheQueryable()
                    .Where(item => item.Value.Type == type && item.Value.Name == name)
                    .Select(c => c.Value)
                    .FirstOrDefault();

            if (parameter == null)
            {
                parameter = new WorkflowGlobalParameter
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Type = type,
                    Value = JsonConvert.SerializeObject(value)
                };

                cache.Put(parameter.Id, parameter);
            }
            else
            {
                parameter.Value = JsonConvert.SerializeObject(value);
                cache.Put(parameter.Id, parameter);
            }
        }

        public T LoadGlobalParameter<T>(string type, string name)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowGlobalParameter>(IgniteConstants.WorkflowGlobalParameterCacheName);

            var parameter =
                cache.AsCacheQueryable()
                    .Where(item => item.Value.Type == type && item.Value.Name == name)
                    .Select(c => c.Value)
                    .FirstOrDefault();

            if (parameter != null)
                return JsonConvert.DeserializeObject<T>(parameter.Value);

            return default(T);
        }

        public List<T> LoadGlobalParameters<T>(string type)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowGlobalParameter>(IgniteConstants.WorkflowGlobalParameterCacheName);

            return
                cache.AsCacheQueryable().Where(item => item.Value.Type == type).ToList()
                    .Select(gp => JsonConvert.DeserializeObject<T>(gp.Value.Value)).ToList();

        }

        public void DeleteGlobalParameters(string type, string name = null)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowGlobalParameter>(IgniteConstants.WorkflowGlobalParameterCacheName);

            var keys = cache.AsCacheQueryable().Where(
                    c => c.Value.Type == type);

            if (!string.IsNullOrEmpty(name))
            {
                keys = keys.Where(c => c.Value.Name == name);
            }
            
            cache.RemoveAll(keys.Select(c=>c.Key).ToList());
        }

        public void DeleteProcess(Guid processId)
        {
            var cacheInstance =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);
            cacheInstance.Remove(processId);

            var cacheStatus =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstanceStatus>(
                    IgniteConstants.WorkflowProcessInstanceStatusCacheName);
            cacheStatus.Remove(processId);

            var cacheTransition =
                Store.GetOrCreateCache<Guid, WorkflowProcessTransitionHistory>(
                    IgniteConstants.WorkflowProcessTransitionHistoryCacheName);
            cacheTransition.RemoveAll(cacheTransition.Where(c => c.Value.ProcessId == processId).Select(c => c.Key));

            var cacheTimer =
                Store.GetOrCreateCache<Guid, WorkflowProcessTimer>(IgniteConstants.WorkflowProcessTimerCacheName);
            cacheTimer.RemoveAll(cacheTransition.Where(c => c.Value.ProcessId == processId).Select(c => c.Key));
        }

        public void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            var cache = Store.GetOrCreateCache<Guid, WorkflowProcessTimer>(IgniteConstants.WorkflowProcessTimerCacheName);
            var timer =
                cache.AsCacheQueryable()
                    .Where(item => item.Value.ProcessId == processId && item.Value.Name == name)
                    .Select(c => c.Value)
                    .FirstOrDefault();
            if (timer == null)
            {
                timer = new WorkflowProcessTimer
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    NextExecutionDateTime = nextExecutionDateTime.ToUniversalTime(),
                    ProcessId = processId,
                    Ignore = false
                };

                cache.Put(timer.Id, timer);
            }
            else if (!notOverrideIfExists)
            {
                timer.NextExecutionDateTime = nextExecutionDateTime.ToUniversalTime();
                cache.Put(timer.Id, timer);
            }
        }

        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            var cacheTimer =
                Store.GetOrCreateCache<Guid, WorkflowProcessTimer>(IgniteConstants.WorkflowProcessTimerCacheName);

            var timers =
            cacheTimer.AsCacheQueryable()
                .Where(c => c.Value.ProcessId == processId);

            foreach (var ignore in timersIgnoreList)
            {
                timers = timers.Where(c => c.Value.Name != ignore);
            }


            var timersIds = timers.Select(c=>c.Key).ToList();

            cacheTimer.RemoveAll(timersIds);
        }

        public void ClearTimersIgnore()
        {
            var cache = Store.GetOrCreateCache<Guid, WorkflowProcessTimer>(IgniteConstants.WorkflowProcessTimerCacheName);
            var timers = cache.AsCacheQueryable().Where(c => c.Value.Ignore).Select(c => c.Value).ToList();
            foreach (var timer in timers)
                timer.Ignore = false;
            cache.PutAll(timers.ToDictionary(c => c.Id));
        }

        public void ClearTimer(Guid timerId)
        {
            var cacheTimer =
                Store.GetOrCreateCache<Guid, WorkflowProcessTimer>(IgniteConstants.WorkflowProcessTimerCacheName);
            cacheTimer.Remove(timerId);
        }

        public DateTime? GetCloseExecutionDateTime()
        {
            var cache = Store.GetOrCreateCache<Guid, WorkflowProcessTimer>(IgniteConstants.WorkflowProcessTimerCacheName);
            var timer =
                cache.AsCacheQueryable()
                    .Where(item => !item.Value.Ignore)
                    .OrderBy(item => item.Value.NextExecutionDateTime)
                    .Select(c => c.Value)
                    .FirstOrDefault();

            if (timer == null)
                return null;

            if (_runtime.UseUtcDateTimeAsRuntimeTime)
                return timer.NextExecutionDateTime;

            return timer.NextExecutionDateTime.ToLocalTime();

        }

        public List<TimerToExecute> GetTimersToExecute()
        {
            var now = _runtime.RuntimeDateTimeNow.ToUniversalTime();
            using (var tx = Store.GetTransactions().TxStart())
            {
                var cache =
                    Store.GetOrCreateCache<Guid, WorkflowProcessTimer>(IgniteConstants.WorkflowProcessTimerCacheName);
                var timers =
                    cache.AsCacheQueryable()
                        .Where(item => !item.Value.Ignore && item.Value.NextExecutionDateTime <= now)
                        .Select(c => c.Value).ToList();

                foreach (var timer in timers)
                    timer.Ignore = true;

                cache.PutAll(timers.ToDictionary(c => c.Id));

                tx.Commit();
                return
                    timers.Select(t => new TimerToExecute {Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id})
                        .ToList();
            }
        }

        #endregion

        #region ISchemePersistenceProvider

        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            WorkflowProcessInstance processInstance = null;
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessInstance>(IgniteConstants.WorkflowProcessInstanceCacheName);

            try
            {
                processInstance = cache.Get(processId);
            }
            catch (KeyNotFoundException)
            {
            }

            if (processInstance == null)
                throw new ProcessNotFoundException(processId);

            if (!processInstance.SchemeId.HasValue)
                throw SchemeNotFoundException.Create(processId, SchemeLocation.WorkflowProcessInstance);

            var schemeDefinition = GetProcessSchemeBySchemeId(processInstance.SchemeId.Value);
            schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
            return schemeDefinition;
        }

        public SchemeDefinition<XElement> GetProcessSchemeBySchemeId(Guid schemeId)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessScheme>(IgniteConstants.WorkflowProcessSchemeCacheName);

            WorkflowProcessScheme processScheme = null;
            try
            {
                processScheme = cache.Get(schemeId);
            }
            catch (KeyNotFoundException)
            {
            }

            if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

            return ConvertToSchemeDefinition(processScheme);
        }


        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, string definingParameters,
            Guid? rootSchemeId, bool ignoreObsolete)
        {
            var hash = HashHelper.GenerateStringHash(definingParameters);

            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessScheme>(IgniteConstants.WorkflowProcessSchemeCacheName);
            var processSchemesQuery = ignoreObsolete
                ? cache.AsCacheQueryable().Where(
                    pss =>
                        pss.Value.SchemeCode == schemeCode && pss.Value.DefiningParametersHash == hash &&
                        !pss.Value.IsObsolete)
                : cache.AsCacheQueryable().Where(
                    pss =>
                        pss.Value.SchemeCode == schemeCode && pss.Value.DefiningParametersHash == hash
                       );

            if (rootSchemeId.HasValue)
            {
                var rootSchemeIdValue = rootSchemeId.Value.ToString("N");
                processSchemesQuery = processSchemesQuery.Where(c => c.Value.RootSchemeId == rootSchemeIdValue);
            }

            var processSchemes = processSchemesQuery.Select(c=>c.Value).ToList();

            if (!processSchemes.Any())
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme,
                    definingParameters);

            if (processSchemes.Count() == 1)
            {
                var scheme = processSchemes.First();
                return ConvertToSchemeDefinition(scheme);
            }

            foreach (
                var processScheme in
                processSchemes.Where(processScheme => processScheme.DefiningParameters == definingParameters))
            {
                return ConvertToSchemeDefinition(processScheme);
            }

            throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme,
                definingParameters);
        }

        public void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessScheme>(IgniteConstants.WorkflowProcessSchemeCacheName);
            var schemes = cache.AsCacheQueryable().Where(
                item =>
                    (item.Value.SchemeCode == schemeCode || item.Value.RootSchemeCode == schemeCode) &&
                    item.Value.DefiningParametersHash == definingParametersHash).Select(c => c.Value).ToList();

            foreach (var scheme in schemes)
                scheme.IsObsolete = true;

            cache.PutAll(schemes.ToDictionary(c => c.Id));
        }

        public void SetSchemeIsObsolete(string schemeCode)
        {
            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessScheme>(IgniteConstants.WorkflowProcessSchemeCacheName);
            var schemes =
                cache.AsCacheQueryable()
                    .Where(item => item.Value.SchemeCode == schemeCode || item.Value.RootSchemeCode == schemeCode)
                    .Select(c => c.Value).ToList();
            foreach (var scheme in schemes)
                scheme.IsObsolete = true;

            cache.PutAll(schemes.ToDictionary(c => c.Id));
        }

        public void SaveScheme(SchemeDefinition<XElement> scheme)
        {
            var definingParameters = scheme.DefiningParameters;
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            var cache =
                Store.GetOrCreateCache<Guid, WorkflowProcessScheme>(IgniteConstants.WorkflowProcessSchemeCacheName);
            var schemeCode = scheme.SchemeCode;
            var isObsolete = scheme.IsObsolete;
            var oldSchemes =
                cache.AsCacheQueryable().Where(
                    wps =>
                        wps.Value.DefiningParametersHash == definingParametersHash &&
                        wps.Value.SchemeCode == schemeCode && wps.Value.IsObsolete == isObsolete).ToList();

            if (oldSchemes.Any())
            {
                if (oldSchemes.Any(oldScheme => oldScheme.Value.DefiningParameters == definingParameters))
                {
                    throw SchemeAlredyExistsException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme, definingParameters);
                }
            }

            var newProcessScheme = new WorkflowProcessScheme
            {
                Id = scheme.Id,
                DefiningParameters = definingParameters,
                DefiningParametersHash = definingParametersHash,
                Scheme = scheme.Scheme.ToString(),
                SchemeCode = schemeCode,
                RootSchemeCode = scheme.RootSchemeCode,
                RootSchemeId = scheme.RootSchemeId.HasValue ? scheme.RootSchemeId.Value.ToString("N") : null,
                AllowedActivities = JsonConvert.SerializeObject(scheme.AllowedActivities),
                StartingTransition = scheme.StartingTransition,
                IsObsolete = scheme.IsObsolete
            };

            cache.Put(newProcessScheme.Id, newProcessScheme);
        }

        public void SaveScheme(string schemaCode, string scheme)
        {
            var cache = Store.GetOrCreateCache<string, WorkflowScheme>(IgniteConstants.WorkflowSchemeCacheName);

            WorkflowScheme wfScheme = cache.ContainsKey(schemaCode) ? cache.Get(schemaCode) : null;

            if (wfScheme == null)
            {
                wfScheme = new WorkflowScheme {Code = schemaCode, Scheme = scheme};
                cache.Put(wfScheme.Code, wfScheme);
            }
            else
            {
                wfScheme.Scheme = scheme;
                cache.Put(wfScheme.Code, wfScheme);
            }
        }


        public XElement GetScheme(string code)
        {
            var cache = Store.GetOrCreateCache<string, WorkflowScheme>(IgniteConstants.WorkflowSchemeCacheName);


            WorkflowScheme scheme = null;

            try
            {
                scheme = cache.Get(code);
            }
            catch (KeyNotFoundException)
            {
            }

            if (scheme == null || string.IsNullOrEmpty(scheme.Scheme))
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);

            return XElement.Parse(scheme.Scheme);
        }

        #endregion

        #region IWorkflowGenerator

        protected IDictionary<string, string> TemplateTypeMapping = new Dictionary<string, string>();

        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
                throw new InvalidOperationException("Parameters not supported");

            var code = !TemplateTypeMapping.ContainsKey(schemeCode.ToLower())
                ? schemeCode
                : TemplateTypeMapping[schemeCode.ToLower()];
            WorkflowScheme scheme;
            var cache = Store.GetOrCreateCache<string, WorkflowScheme>(IgniteConstants.WorkflowSchemeCacheName);
            {
                scheme = cache.Get(code);
            }

            if (scheme == null)
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);

            return XElement.Parse(scheme.Scheme);
        }

        public void AddMapping(string processName, object generatorSource)
        {
            var value = generatorSource as string;
            if (value == null)
                throw new InvalidOperationException("Generator source must be a string");
            TemplateTypeMapping.Add(processName.ToLower(), value);
        }

        #endregion

        public static IgniteConfiguration GetDefaultIgniteConfiguration()
        {
            var cfg = new IgniteConfiguration
            {
                BinaryConfiguration = new BinaryConfiguration
                {
                    TypeConfigurations = new[]
                    {
                        new BinaryTypeConfiguration(typeof(WorkflowGlobalParameter)),
                        new BinaryTypeConfiguration(typeof(WorkflowProcessInstance)),
                        new BinaryTypeConfiguration(typeof(WorkflowProcessInstancePersistence)),
                        new BinaryTypeConfiguration(typeof(WorkflowProcessInstanceStatus)),
                        new BinaryTypeConfiguration(typeof(WorkflowProcessScheme)),
                        new BinaryTypeConfiguration(typeof(WorkflowProcessTimer)),
                        new BinaryTypeConfiguration(typeof(WorkflowProcessTransitionHistory)),
                        new BinaryTypeConfiguration(typeof(WorkflowScheme)),
                    },


                },

                CacheConfiguration = new[]
                {
                    new CacheConfiguration(IgniteConstants.WorkflowProcessInstanceStatusCacheName,
                        typeof(WorkflowProcessInstanceStatus))
                    {
                        WriteSynchronizationMode = CacheWriteSynchronizationMode.PrimarySync,
                        CacheMode = CacheMode.Partitioned
                    },
                    new CacheConfiguration(IgniteConstants.WorkflowGlobalParameterCacheName,
                        typeof(WorkflowGlobalParameter))
                    {
                        WriteSynchronizationMode = CacheWriteSynchronizationMode.PrimarySync,
                        CacheMode = CacheMode.Partitioned
                    },
                    new CacheConfiguration(IgniteConstants.WorkflowProcessTimerCacheName, typeof(WorkflowProcessTimer))
                    {
                        WriteSynchronizationMode = CacheWriteSynchronizationMode.PrimarySync,
                        CacheMode = CacheMode.Partitioned
                    },
                    new CacheConfiguration(IgniteConstants.WorkflowProcessSchemeCacheName, typeof(WorkflowProcessScheme))
                    {
                        WriteSynchronizationMode = CacheWriteSynchronizationMode.FullSync,
                        CacheMode = CacheMode.Partitioned
                    },

                },
                TransactionConfiguration =
                    new TransactionConfiguration
                    {
                        DefaultTransactionConcurrency = TransactionConcurrency.Pessimistic,
                        DefaultTransactionIsolation = TransactionIsolation.ReadCommitted
                    }


            };

            return cfg;
        }
    }
}