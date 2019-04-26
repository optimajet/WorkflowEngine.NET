using System;
using System.Collections.Generic;
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
using StackExchange.Redis;

namespace OptimaJet.Workflow.Redis
{
    public class RedisProvider : IWorkflowProvider
    {
        private readonly ConnectionMultiplexer _connector;
        private readonly string _providerNamespace;
        private WorkflowRuntime _runtime;

        public RedisProvider(ConnectionMultiplexer connector, string providerNamespace = "wfe")
        {
            _connector = connector;
            _providerNamespace = providerNamespace;
        }



        #region Implementation of IWorkflowGenerator<out XElement>

        /// <summary>
        /// Generate not parsed process scheme
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="parameters">Parameters for creating scheme</param>
        /// <returns>Not parsed process scheme</returns>
        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
                throw new InvalidOperationException("Parameters not supported");

            return GetScheme(schemeCode);
        }

        #endregion

        #region Key generators

        public string GetKeyForScheme(string schemeCode)
        {
            return string.Format("{0}:scheme:{1}", _providerNamespace, schemeCode);
        }

        public string GetKeyForProcessInstance(Guid processId)
        {
            return string.Format("{0}:processinstance:{1:N}", _providerNamespace, processId);
        }

        public string GetKeyForProcessHistory(Guid processId)
        {
            return string.Format("{0}:processhistory:{1:N}", _providerNamespace, processId);
        }

        public string GetKeyForProcessPersistence(Guid processId)
        {
            return string.Format("{0}:processpersistence:{1:N}", _providerNamespace, processId);
        }

        public string GetKeyForProcessScheme(Guid schemeId)
        {
            return string.Format("{0}:processscheme:{1:N}", _providerNamespace, schemeId);
        }

        public string GetKeyForCurrentScheme(string schemeCode)
        {
            return string.Format("{0}:currentscheme:{1}", _providerNamespace, schemeCode);
        }

        public string GetKeySchemeHierarchy(Guid schemeId)
        {
            return string.Format("{0}:schemehierarchy:{1:N}", _providerNamespace, schemeId);
        }

        public string GetKeyProcessRunning()
        {
            return string.Format("{0}:processrunning", _providerNamespace);
        }

        public string GetKeyProcessStatus()
        {
            return string.Format("{0}:processstatus", _providerNamespace);
        }

        public string GetKeyProcessTimer(Guid processId)
        {
            return string.Format("{0}:processtimer:{1:N}", _providerNamespace, processId);
        }

        public string GetKeyTimerTime()
        {
            return string.Format("{0}:timertime", _providerNamespace);
        }

        public string GetKeyTimer()
        {
            return string.Format("{0}:timer", _providerNamespace);
        }

        public string GetKeyTimerIgnore()
        {
            return string.Format("{0}:timerignore", _providerNamespace);
        }

        public string GetKeyGlobalParameter(string type)
        {
            return string.Format("{0}:globalparameter:{1}", _providerNamespace, type);
        }

        public string GetKeyCanBeInlined()
        {
            return string.Format("{0}:schemecanbeinlined", _providerNamespace); 
        }
        
        public string GetKeyForInlined()
        {
            return string.Format("{0}:schemeinlined", _providerNamespace); 
        }
        
        #endregion

        #region Private

        private SchemeDefinition<XElement> ConvertToSchemeDefinition(Guid schemeId, bool isObsolete, WorkflowProcessScheme workflowProcessScheme)
        {
            return new SchemeDefinition<XElement>(schemeId, workflowProcessScheme.RootSchemeId,
                workflowProcessScheme.SchemeCode, workflowProcessScheme.RootSchemeCode,
                XElement.Parse(workflowProcessScheme.Scheme), isObsolete, false,
                workflowProcessScheme.AllowedActivities, workflowProcessScheme.StartingTransition,
                workflowProcessScheme.DefiningParameters);
        }
        
        private void SetRunningStatus(Guid processId)
        {
            var db = _connector.GetDatabase();
            var hash = string.Format("{0:N}", processId);

            if (!db.HashExists(GetKeyProcessStatus(), hash))
                throw new StatusNotDefinedException();

            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.HashNotExists(GetKeyProcessRunning(), hash));
            tran.HashSetAsync(GetKeyProcessRunning(), hash, true);
            tran.HashSetAsync(GetKeyProcessStatus(), hash, ProcessStatus.Running.Id);

            var res = tran.Execute();

            if (!res)
                throw new ImpossibleToSetStatusException();
        }
        
        private void SetCustomStatus(Guid processId, ProcessStatus status, bool createIfnotDefined = false)
        {
            var db = _connector.GetDatabase();
            var hash = string.Format("{0:N}", processId);
            if (!createIfnotDefined && !db.HashExists(GetKeyProcessStatus(), hash))
                throw new StatusNotDefinedException();
            var batch = db.CreateBatch();
            batch.HashDeleteAsync(GetKeyProcessRunning(), hash);
            batch.HashSetAsync(GetKeyProcessStatus(), hash, status.Id);
            batch.Execute();
        }

        private void AddDeleteTimersOperationsToBatch(Guid processId, List<string> timersIgnoreList, IDatabase db, IBatch batch)
        {
            var keyProcessTimer = GetKeyProcessTimer(processId);
            var timers =
                db.HashGetAll(keyProcessTimer)
                    .Where(he => !timersIgnoreList.Contains(he.Name))
                    .Select(he => new TimerToExecute {Name = he.Name, ProcessId = processId, TimerId = Guid.Parse(he.Value)})
                    .ToList();

            foreach (var timer in timers)
            {
                batch.SortedSetRemoveAsync(GetKeyTimerTime(), timer.TimerId.ToString("N"));
                batch.HashDeleteAsync(keyProcessTimer, timer.Name);
                batch.HashDeleteAsync(GetKeyTimerIgnore(), timer.TimerId.ToString("N"));
                batch.HashDeleteAsync(GetKeyTimer(), timer.TimerId.ToString("N"));
            }
        }

        #endregion

        #region Implementation of ISchemePersistenceProvider<XElement>

        /// <summary>
        /// Gets not parsed scheme of the process by process id
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="ProcessNotFoundException"></exception>
        /// <exception cref="SchemeNotFoundException"></exception>
        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            var db = _connector.GetDatabase();
            var processInstanceValue = db.StringGet(GetKeyForProcessInstance(processId));

            if (!processInstanceValue.HasValue)
                throw new ProcessNotFoundException(processId);

            var processInstance = JsonConvert.DeserializeObject<WorkflowProcessInstance>(processInstanceValue);

            if (!processInstance.SchemeId.HasValue)
                throw SchemeNotFoundException.Create(processId, SchemeLocation.WorkflowProcessInstance);

            var schemeDefinition = GetProcessSchemeBySchemeId(processInstance.SchemeId.Value);
            schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
            return schemeDefinition;
        }

        /// <summary>
        /// Gets not parsed scheme by id
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
        public SchemeDefinition<XElement> GetProcessSchemeBySchemeId(Guid schemeId)
        {
            var db = _connector.GetDatabase();
            var processSchemeValue = db.StringGet(GetKeyForProcessScheme(schemeId));

            if (!processSchemeValue.HasValue)
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

            var processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(processSchemeValue);

            if (string.IsNullOrEmpty(processScheme.Scheme))
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

            var key = GetKeyForCurrentScheme(processScheme.RootSchemeId.HasValue ? processScheme.RootSchemeCode : processScheme.SchemeCode);

            var isObsolete = !db.HashExists(key, processScheme.DefiningParameters);

            return ConvertToSchemeDefinition(schemeId, isObsolete, processScheme);
        }

        /// <summary>
        /// Gets not parsed scheme by scheme name and parameters    
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">Parameters for creating the scheme</param>
        /// <param name="rootSchemeId">Id of the root scheme in case of subprocess</param>
        /// <param name="ignoreObsolete">True if you need to ignore obsolete schemes</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, string parameters, Guid? rootSchemeId, bool ignoreObsolete)
        {
            var db = _connector.GetDatabase();

            if (rootSchemeId.HasValue)
            {
                var schemeIdValue = db.HashGet(GetKeySchemeHierarchy(rootSchemeId.Value), schemeCode);
                if (!schemeIdValue.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
                }
                var schemeId = Guid.Parse(schemeIdValue);

                var processSchemeValue = db.StringGet(GetKeyForProcessScheme(schemeId));

                if (!processSchemeValue.HasValue)
                    throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

                var processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(processSchemeValue);
                var isObsolete = !db.HashExists(GetKeyForCurrentScheme(processScheme.RootSchemeCode), parameters);

                if (!ignoreObsolete && isObsolete)
                {
                    throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
                }

                return ConvertToSchemeDefinition(schemeId, isObsolete, processScheme);
            }
            else
            {
                var schemeIdValue = db.HashGet(GetKeyForCurrentScheme(schemeCode), parameters);

                if (!schemeIdValue.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
                }

                var schemeId = Guid.Parse(schemeIdValue);

                var processSchemeValue = db.StringGet(GetKeyForProcessScheme(schemeId));

                if (!processSchemeValue.HasValue)
                    throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

                var processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(processSchemeValue);

                return ConvertToSchemeDefinition(schemeId, false, processScheme);
            }
        }

        /// <summary>
        /// Gets not parsed scheme by scheme name  
        /// </summary>
        /// <param name="code">Name of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
        public XElement GetScheme(string code)
        {
            var scheme = _connector.GetDatabase().StringGet(GetKeyForScheme(code));

            if (!scheme.HasValue || string.IsNullOrEmpty(scheme)) //-V3027
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);

            return XElement.Parse(scheme);
        }

        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="scheme">Not parsed scheme of the process</param>
        /// <exception cref="SchemeAlreadyExistsException"></exception>
        public void SaveScheme(SchemeDefinition<XElement> scheme)
        {
            var db = _connector.GetDatabase();


            var tran = db.CreateTransaction();

            if (!scheme.IsObsolete) //there is only one current scheme can exists 
            {
                if (!scheme.RootSchemeId.HasValue)
                {
                    var key = GetKeyForCurrentScheme(scheme.SchemeCode);
                    var hash = scheme.DefiningParameters;
                    tran.AddCondition(Condition.HashNotExists(key, hash));
                    tran.HashSetAsync(key, hash, string.Format("{0:N}", scheme.Id));
                }
            }

            var newProcessScheme = new WorkflowProcessScheme
            {
                DefiningParameters = scheme.DefiningParameters,
                Scheme = scheme.Scheme.ToString(),
                SchemeCode = scheme.SchemeCode,
                RootSchemeCode = scheme.RootSchemeCode,
                RootSchemeId = scheme.RootSchemeId,
                AllowedActivities = scheme.AllowedActivities,
                StartingTransition = scheme.StartingTransition
            };

            var newSchemeValue = JsonConvert.SerializeObject(newProcessScheme);
            var newSchemeKey = GetKeyForProcessScheme(scheme.Id);

            

            tran.AddCondition(Condition.KeyNotExists(newSchemeKey));

            tran.StringSetAsync(newSchemeKey, newSchemeValue);

            if (scheme.RootSchemeId.HasValue)
            {
                tran.HashSetAsync(GetKeySchemeHierarchy(scheme.RootSchemeId.Value), scheme.SchemeCode, scheme.Id.ToString("N"));
            }

            var result = tran.Execute();

            if (!result)
                throw SchemeAlreadyExistsException.Create(scheme.SchemeCode, SchemeLocation.WorkflowProcessScheme, scheme.DefiningParameters);
        }



        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">Parameters for creating the scheme</param>
        public void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            var db = _connector.GetDatabase();
            db.HashDelete(GetKeyForCurrentScheme(schemeCode), definingParameters);
        }

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        public void SetSchemeIsObsolete(string schemeCode)
        {
            var db = _connector.GetDatabase();
            db.KeyDelete(GetKeyForCurrentScheme(schemeCode));
        }


        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="inlinedSchemes">Scheme codes to be inlined into this scheme</param>
        /// <param name="scheme">Not parsed scheme</param>
        /// <param name="canBeInlined">if true - this scheme can be inlined into another schemes</param>
        public void SaveScheme(string schemeCode, bool canBeInlined, List<string> inlinedSchemes, string scheme)
        {
            var db = _connector.GetDatabase();
            var tran = db.CreateTransaction();
            tran.StringSetAsync(GetKeyForScheme(schemeCode), scheme);

            var keyForInlined = GetKeyForInlined();
            var keyCanBeInlined = GetKeyCanBeInlined();
            
            if (inlinedSchemes == null || !inlinedSchemes.Any())
            {
                tran.HashDeleteAsync(keyForInlined,schemeCode);
            }
            else
            {
                tran.HashSetAsync(keyForInlined, schemeCode, JsonConvert.SerializeObject(inlinedSchemes));
            }
            
            //can be inlined
            if (canBeInlined)
            {
                tran.HashSetAsync(keyCanBeInlined, schemeCode, true);
            }
            else
            {
                tran.HashDeleteAsync(keyCanBeInlined, schemeCode);
            }
            
            tran.Execute();
        }

        public List<string> GetInlinedSchemeCodes()
        {
            var keys = _connector.GetDatabase().HashKeys(GetKeyCanBeInlined());
            return keys.Select(c => c.ToString()).ToList();
        }

        public List<string> GetRelatedByInliningSchemeCodes(string schemeCode)
        {
            var db = _connector.GetDatabase();

            var pairs = db.HashGetAll(GetKeyForInlined());

            var res = new List<string>();
            
            foreach (var pair in pairs)
            {
                var inlined = JsonConvert.DeserializeObject<List<string>>(pair.Value.ToString());
                if (inlined.Contains(schemeCode))
                    res.Add(pair.Name.ToString());
            }

            return res;
        }

        #endregion

        #region Implementation of IPersistenceProvider

        /// <summary>
        /// Init the provider
        /// </summary>
        /// <param name="runtime">Workflow runtime instance which owned the provider</param>
        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Initialize a process instance in persistence store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ProcessAlreadyExistsException"></exception>
        public void InitializeProcess(ProcessInstance processInstance)
        {
            var newProcess = new WorkflowProcessInstance
            {
                Id = processInstance.ProcessId,
                SchemeId = processInstance.SchemeId,
                ActivityName = processInstance.ProcessScheme.InitialActivity.Name,
                StateName = processInstance.ProcessScheme.InitialActivity.State,
                RootProcessId = processInstance.RootProcessId,
                ParentProcessId = processInstance.ParentProcessId
            };

            var processKey = GetKeyForProcessInstance(processInstance.ProcessId);

            var db = _connector.GetDatabase();
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyNotExists(processKey));
            tran.StringSetAsync(processKey, JsonConvert.SerializeObject(newProcess));

            var res = tran.Execute();

            if (!res)
                throw new ProcessAlreadyExistsException(processInstance.ProcessId);
        }

        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/>  and persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        public void FillProcessParameters(ProcessInstance processInstance)
        {
            FillPersistedProcessParameters(processInstance);
            FillSystemProcessParameters(processInstance);
        }

        /// <summary>
        /// Fills persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        public void FillPersistedProcessParameters(ProcessInstance processInstance)
        {
            var persistenceParameters = processInstance.ProcessScheme.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count());

            var db = _connector.GetDatabase();
            var persistedParametersValue = db.StringGet(GetKeyForProcessPersistence(processInstance.ProcessId));
            if (!persistedParametersValue.HasValue)
                return;

            var persistedParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(persistedParametersValue);

            foreach (var persistedParameter in persistedParameters)
            {
                var parameterDefinition = persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.Key);
                if (parameterDefinition == null)
                {
                    parameterDefinition = ParameterDefinition.Create(persistedParameter.Key, typeof(UnknownParameterType), ParameterPurpose.Persistence);
                    parameters.Add(ParameterDefinition.Create(parameterDefinition, persistedParameter.Value));
                }
                else
                {
                    parameters.Add(ParameterDefinition.Create(parameterDefinition, ParametersSerializer.Deserialize(persistedParameter.Value, parameterDefinition.Type)));
                }
            }

            processInstance.AddParameters(parameters);
        }

        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        public void FillSystemProcessParameters(ProcessInstance processInstance)
        {
            var db = _connector.GetDatabase();
            var workflowProcessInstanceValue = db.StringGet(GetKeyForProcessInstance(processInstance.ProcessId));

            if (!workflowProcessInstanceValue.HasValue)
                throw new ProcessNotFoundException(processInstance.ProcessId);

            var workflowProcessInstance = JsonConvert.DeserializeObject<WorkflowProcessInstance>(workflowProcessInstanceValue);

            var systemParameters =
                processInstance.ProcessScheme.Parameters.Where(p => p.Purpose == ParameterPurpose.System).ToList();

            var parameters = new List<ParameterDefinitionWithValue>(systemParameters.Count)
            {
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterProcessId.Name),
                    processInstance.ProcessId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousState.Name),
                    workflowProcessInstance.PreviousState),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCurrentState.Name),
                    workflowProcessInstance.StateName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousStateForDirect.Name),
                    workflowProcessInstance.PreviousStateForDirect),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousStateForReverse.Name),
                    workflowProcessInstance.PreviousStateForReverse),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivity.Name),
                    workflowProcessInstance.PreviousActivity),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCurrentActivity.Name),
                    workflowProcessInstance.ActivityName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivityForDirect.Name),
                    workflowProcessInstance.PreviousActivityForDirect),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterPreviousActivityForReverse.Name),
                    workflowProcessInstance.PreviousActivityForReverse),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSchemeCode.Name),
                    processInstance.ProcessScheme.Name),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSchemeId.Name),
                    workflowProcessInstance.SchemeId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterIsPreExecution.Name),
                    false),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterParentProcessId.Name),
                    workflowProcessInstance.ParentProcessId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterRootProcessId.Name),
                    workflowProcessInstance.RootProcessId)
            };

            processInstance.AddParameters(parameters);
        }

        /// <summary>
        /// Saves persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process to store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        public void SavePersistenceParameters(ProcessInstance processInstance)
        {
            var parametersToSave = processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Value != null)
                .ToDictionary(ptp => ptp.Name, ptp =>
                {
                    if (ptp.Type == typeof(UnknownParameterType))
                        return (string) ptp.Value;
                    return ParametersSerializer.Serialize(ptp.Value, ptp.Type);

                });

            var parametersToRemove =
                processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Value == null).Select(ptp => ptp.Name).ToList();

            if (!parametersToSave.Any() && !parametersToRemove.Any())
                return;

            var db = _connector.GetDatabase();

            var key = GetKeyForProcessPersistence(processInstance.ProcessId);

            var oldPrarmetersValue = db.StringGet(key);

            if (!oldPrarmetersValue.HasValue)
            {
                if (parametersToSave.Any())
                    db.StringSet(key, JsonConvert.SerializeObject(parametersToSave));
            }
            else
            {
                var existingParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(oldPrarmetersValue);

                parametersToRemove.ForEach(p => existingParameters.Remove(p));

                foreach (var ptsKey in parametersToSave.Keys)
                {
                    if (existingParameters.ContainsKey(ptsKey))
                    {
                        existingParameters[ptsKey] = parametersToSave[ptsKey];
                    }
                    else
                    {
                        existingParameters.Add(ptsKey, parametersToSave[ptsKey]);
                    }
                }

                if (existingParameters.Any())
                    db.StringSet(key, JsonConvert.SerializeObject(existingParameters));
                else
                    db.KeyDelete(key);
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

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Initialized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        public void SetWorkflowIniialized(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Initialized, true);
        }



        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Idled"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        public void SetWorkflowIdled(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Idled);
        }

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Running"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        public void SetWorkflowRunning(ProcessInstance processInstance)
        {
            var processId = processInstance.ProcessId;
            SetRunningStatus(processId);
        }

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Finalized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
        public void SetWorkflowFinalized(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Finalized);
        }

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Terminated"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
#pragma warning disable 612
        public void SetWorkflowTerminated(ProcessInstance processInstance, ErrorLevel level, string errorMessage)
#pragma warning restore 612
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Terminated);
        }

        /// <summary>
        /// Resets all process to <see cref="ProcessStatus.Idled"/> status
        /// </summary>
        public void ResetWorkflowRunning()
        {
            var db = _connector.GetDatabase();
            var allRunningIds = db.HashKeys(GetKeyProcessRunning());

            var batch = db.CreateBatch();
            foreach (var hash in allRunningIds.Where(ari => ari.HasValue))
            {
                batch.HashSetAsync(GetKeyProcessStatus(), hash, ProcessStatus.Idled.Id);
            }
            batch.KeyDeleteAsync(GetKeyProcessRunning());

            batch.Execute();
        }

        /// <summary>
        /// Updates system parameters of the process in the store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <param name="transition">Last executed transition</param>
        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var db = _connector.GetDatabase();
            var key = GetKeyForProcessInstance(processInstance.ProcessId);
            var processInstanceValue = db.StringGet(key);

            if (!processInstanceValue.HasValue)
                throw new ProcessNotFoundException(processInstance.ProcessId);

            var inst = JsonConvert.DeserializeObject<WorkflowProcessInstance>(processInstanceValue);

            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);
            var identityId = paramIdentityId == null ? string.Empty : (string) paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string) paramImpIdentityId.Value;

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

            var history = new WorkflowProcessTransitionHistory()
            {
                ActorIdentityId = impIdentityId,
                ExecutorIdentityId = identityId,
                IsFinalised = transition.To.IsFinal,
                FromActivityName = transition.From.Name,
                FromStateName = transition.From.State,
                ToActivityName = transition.To.Name,
                ToStateName = transition.To.State,
                TransitionClassifier =
                    transition.Classifier.ToString(),
                TransitionTime = _runtime.RuntimeDateTimeNow,
                TriggerName = string.IsNullOrEmpty(processInstance.ExecutedTimer) ? processInstance.CurrentCommand : processInstance.ExecutedTimer
            };

            var batch = db.CreateBatch();
            batch.StringSetAsync(key, JsonConvert.SerializeObject(inst));
            batch.ListRightPushAsync(GetKeyForProcessHistory(processInstance.ProcessId), JsonConvert.SerializeObject(history));
            batch.Execute();
        }

        /// <summary>
        /// Checks existence of the process
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
        public bool IsProcessExists(Guid processId)
        {
            var db = _connector.GetDatabase();
            return db.KeyExists(GetKeyForProcessInstance(processId));
        }

        /// <summary>
        /// Returns status of the process <see cref="ProcessStatus"/>
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns>Status of the process</returns>
        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            var db = _connector.GetDatabase();
            var statusValue = db.HashGet(GetKeyProcessStatus(), string.Format("{0:N}", processId));
            if (!statusValue.HasValue)
                return ProcessStatus.NotFound;
            var statusId = int.Parse(statusValue);
            var status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == statusId);
            return status ?? ProcessStatus.Unknown;
        }

        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process whith changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            BindProcessToNewScheme(processInstance, false);
        }

        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process whith changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
        /// <param name="resetIsDeterminingParametersChanged">True if required to reset IsDeterminingParametersChanged flag <see cref="ProcessInstance.IsDeterminingParametersChanged"/></param>
        /// <exception cref="ProcessNotFoundException"></exception>
        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            var db = _connector.GetDatabase();
            var key = GetKeyForProcessInstance(processInstance.ProcessId);
            var processInstanceValue = db.StringGet(key);

            if (!processInstanceValue.HasValue)
                throw new ProcessNotFoundException(processInstance.ProcessId);

            var inst = JsonConvert.DeserializeObject<WorkflowProcessInstance>(processInstanceValue);

            inst.SchemeId = processInstance.SchemeId;
            if (resetIsDeterminingParametersChanged)
                inst.IsDeterminingParametersChanged = false;

            db.StringSet(key, JsonConvert.SerializeObject(inst));
        }

        /// <summary>
        /// Register a new timer
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <param name="name">Timer name <see cref="TimerDefinition.Name"/></param>
        /// <param name="nextExecutionDateTime">Next date and time of timer's execution</param>
        /// <param name="notOverrideIfExists">If true specifies that the existing timer with same name will not be overriden <see cref="TimerDefinition.NotOverrideIfExists"/></param>
        public void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            var db = _connector.GetDatabase();
            var unixTime = (nextExecutionDateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;


            var timerId = Guid.NewGuid();
            var timerToExecute = new TimerToExecute() {ProcessId = processId, Name = name, TimerId = timerId};

            var tran = db.CreateTransaction();

            if (notOverrideIfExists)
            {
                tran.AddCondition(Condition.HashNotExists(GetKeyProcessTimer(processId), name));
            }

            tran.SortedSetAddAsync(GetKeyTimerTime(), timerId.ToString("N"), unixTime);
            tran.HashSetAsync(GetKeyProcessTimer(processId), name, timerId.ToString("N"));
            tran.HashSetAsync(GetKeyTimer(), timerId.ToString("N"), JsonConvert.SerializeObject(timerToExecute));

            tran.Execute();
        }

        /// <summary>
        /// Removes all timers from the store, exlude listed in ignore list
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <param name="timersIgnoreList">Ignore list</param>
        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            var db = _connector.GetDatabase();
            var batch = db.CreateBatch();

            AddDeleteTimersOperationsToBatch(processId, timersIgnoreList, db, batch);

            batch.Execute();
        }

        /// <summary>
        /// Clears sign Ignore for all timers
        /// </summary>
        public void ClearTimersIgnore()
        {
            var db = _connector.GetDatabase();
            db.KeyDelete(GetKeyTimerIgnore());
        }

        /// <summary>
        /// Clears sign Ignore for specific timers
        /// </summary>
        public void ClearTimerIgnore(Guid timerId)
        {
            var db = _connector.GetDatabase();
            var keyTimerIgnore = GetKeyTimerIgnore();
            db.HashDelete(keyTimerIgnore, timerId.ToString("N"));
        }

        /// <summary>
        /// Remove specific timer
        /// </summary>
        /// <param name="timerId">Id of the timer</param>
        public void ClearTimer(Guid timerId)
        {
            var db = _connector.GetDatabase();
            var keyTimer = GetKeyTimer();
            var keyTimerTime = GetKeyTimerTime();
            var keyTimerIgnore = GetKeyTimerIgnore();

            var timerValue = db.HashGet(keyTimer, timerId.ToString("N"));
            if (!timerValue.HasValue)
            {
                var batchTimerNotExists = db.CreateBatch();
                batchTimerNotExists.SortedSetRemoveAsync(keyTimerTime, timerId.ToString("N"));
                batchTimerNotExists.HashDeleteAsync(keyTimerIgnore, timerId.ToString("N"));
                batchTimerNotExists.Execute();
                return;
            }

            var timer = JsonConvert.DeserializeObject<TimerToExecute>(timerValue);
            var keyProcessTimer = GetKeyProcessTimer(timer.ProcessId);
            var timerInProcessIdValue = db.HashGet(keyProcessTimer, timer.Name);

            var batch = db.CreateBatch();

            batch.SortedSetRemoveAsync(keyTimerTime, timerId.ToString("N"));

            if (timerInProcessIdValue.HasValue && Guid.Parse(timerInProcessIdValue) == timerId)
                batch.HashDeleteAsync(keyProcessTimer, timer.Name);
            batch.HashDeleteAsync(keyTimerIgnore, timerId.ToString("N"));
            batch.HashDeleteAsync(keyTimer, timerId.ToString("N"));
            batch.Execute();
        }

        /// <summary>
        /// Get closest execution date and time for all timers
        /// </summary>
        /// <returns></returns>
        public DateTime? GetCloseExecutionDateTime()
        {
            var keyTimerIgnore = GetKeyTimerIgnore();
            var keyTimer = GetKeyTimerTime();
            var db = _connector.GetDatabase();
            var skip = 0;
            while (true)
            {
                var closestTimers = db.SortedSetRangeByScore(keyTimer, double.NegativeInfinity, double.PositiveInfinity, Exclude.None, Order.Ascending, skip, 10);
                if (!closestTimers.Any())
                    return null;

                foreach (var closestTimer in closestTimers)
                {
                    if (!db.HashExists(keyTimerIgnore, closestTimer))
                    {
                        var unixTime = db.SortedSetScore(keyTimer, closestTimer);
                        if (unixTime.HasValue)
                        {
                            var executionTime = new DateTime(1970, 1, 1).AddMilliseconds(unixTime.Value);
                            return _runtime.UseUtcDateTimeAsRuntimeTime
                                ? executionTime.ToUniversalTime()
                                : executionTime.ToLocalTime();
                        }
                    }
                }
                skip += 10;
            }
        }

        /// <summary>
        /// Get all timers which must be executed at this moment of time
        /// </summary>
        /// <returns>List of timers to execute</returns>
        public List<TimerToExecute> GetTimersToExecute()
        {
            var keyTimerIgnore = GetKeyTimerIgnore();
            var unixTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            var db = _connector.GetDatabase();

            var timerIds = db.SortedSetRangeByScore(GetKeyTimerTime(), double.NegativeInfinity, unixTime).Where(rv => rv.HasValue);
            var result = new List<TimerToExecute>();
            foreach (var timerIdValue in timerIds)
            {
                var tran = db.CreateTransaction();
                tran.AddCondition(Condition.HashNotExists(keyTimerIgnore, timerIdValue));
                tran.HashSetAsync(keyTimerIgnore, timerIdValue, true);
                var res = tran.Execute();
                if (res)
                {
                    var timerValue = db.HashGet(GetKeyTimer(), timerIdValue);
                    if (timerValue.HasValue)
                    {
                        result.Add(JsonConvert.DeserializeObject<TimerToExecute>(timerValue));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processId">Id of the process</param>
        public void DeleteProcess(Guid processId)
        {
            var db = _connector.GetDatabase();
            var batch = db.CreateBatch();
            AddDeleteTimersOperationsToBatch(processId, new List<string>(), db, batch);
            batch.KeyDeleteAsync(GetKeyForProcessInstance(processId));
            batch.KeyDeleteAsync(GetKeyForProcessPersistence(processId));
            batch.KeyDeleteAsync(GetKeyForProcessHistory(processId));
            var hash = string.Format("{0:N}", processId);
            batch.HashDeleteAsync(GetKeyProcessStatus(), hash);
            batch.HashDeleteAsync(GetKeyProcessRunning(), hash);
            batch.Execute();
        }

        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processIds">List of ids of the process</param>
        public void DeleteProcess(Guid[] processIds)
        {
            foreach (var processId in processIds)
            {
                DeleteProcess(processId);
            }
        }

        /// <summary>
        /// Saves a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        public void SaveGlobalParameter<T>(string type, string name, T value)
        {
            var db = _connector.GetDatabase();
            db.HashSet(GetKeyGlobalParameter(type), name, JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Value of the parameter</returns>
        public T LoadGlobalParameter<T>(string type, string name)
        {
            var db = _connector.GetDatabase();
            var value = db.HashGet(GetKeyGlobalParameter(type), name);
            if (!value.HasValue)
                return default(T);

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <returns>List of the values of the parameters</returns>
        public List<T> LoadGlobalParameters<T>(string type)
        {
            var db = _connector.GetDatabase();
            return db.HashGetAll(GetKeyGlobalParameter(type)).Where(he => he.Value.HasValue).Select(he => JsonConvert.DeserializeObject<T>(he.Value)).ToList();
        }

        /// <summary>
        /// Deletes a global parameter
        /// </summary>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        public void DeleteGlobalParameters(string type, string name = null)
        {
            var db = _connector.GetDatabase();
            if (name != null)
            {
                db.HashDelete(GetKeyGlobalParameter(type), name);
            }
            else
            {
                db.KeyDelete(GetKeyGlobalParameter(type));
            }
        }

        /// <inheritdoc />
        public List<ProcessHistoryItem> GetProcessHistory(Guid processId)
        {
            var db = _connector.GetDatabase();

            return db.ListRange(GetKeyForProcessHistory(processId))
                .Select(hi => JsonConvert.DeserializeObject<WorkflowProcessTransitionHistory>(hi))
                .Select(hi => new ProcessHistoryItem
                {
                    ActorIdentityId = hi.ActorIdentityId,
                    ExecutorIdentityId = hi.ExecutorIdentityId,
                    FromActivityName = hi.FromActivityName,
                    FromStateName = hi.FromStateName,
                    IsFinalised = hi.IsFinalised,
                    ProcessId = processId,
                    ToActivityName = hi.ToActivityName,
                    ToStateName = hi.ToStateName,
                    TransitionClassifier = (TransitionClassifier)Enum.Parse(typeof(TransitionClassifier), hi.TransitionClassifier),
                    TransitionTime = hi.TransitionTime,
                    TriggerName = hi.TriggerName
                })
                .ToList();
        }

        public IEnumerable<ProcessTimer> GetTimersForProcess(Guid processId)
        {
            var db = _connector.GetDatabase();

            var keyProcessTimer = GetKeyProcessTimer(processId);

            var timerIds = db.HashGetAll(keyProcessTimer).Select(he => new { Id = Guid.Parse(he.Value), he.Name });

            List<ProcessTimer> result = new List<ProcessTimer>(timerIds.Count());

            var times = db.SortedSetRangeByRankWithScores(GetKeyTimerTime())
                          .Where(rv => rv.Element.HasValue)
                          .Select(t => new
                          {
                              Id = Guid.Parse(t.Element),
                              Time = t.Score
                          });

            return timerIds.Select(x => new ProcessTimer
            {
                Name = x.Name,
                NextExecutionDateTime = new DateTime(1970, 1, 1).AddMilliseconds(times.First(t => t.Id == x.Id).Time)
            });
  
        }

        #endregion

        #region Bulk methods

        public bool IsBulkOperationsSupported
        {
            get { return false; }
        }

        public async Task BulkInitProcesses(List<ProcessInstance> instances, ProcessStatus status, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task BulkInitProcesses(List<ProcessInstance> instances, List<TimerToRegister> timers, ProcessStatus status, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
