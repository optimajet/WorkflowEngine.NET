using System;
using System.Collections.Generic;
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
using StackExchange.Redis;

namespace OptimaJet.Workflow.Redis
{
    public class RedisProvider : IWorkflowProvider, IApprovalProvider
    {
        private readonly ConnectionMultiplexer _connector;
        private readonly string _providerNamespace;
        private WorkflowRuntime _runtime;
        private readonly bool WriteToHistory;
        private readonly bool WriteSubProcessToRoot;

        public RedisProvider(ConnectionMultiplexer connector, string providerNamespace = "wfe", bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            _connector = connector;
            _providerNamespace = providerNamespace;
            WriteToHistory = writeToHistory;
            WriteSubProcessToRoot = writeSubProcessToRoot;
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
            return $"{_providerNamespace}:scheme:{schemeCode}";
        }

        public string GetKeyForProcessInstance(Guid processId)
        {
            return $"{_providerNamespace}:processinstance:{processId:N}";
        }

        public string GetKeyForProcessHistory(Guid processId)
        {
            return $"{_providerNamespace}:processhistory:{processId:N}";
        }

        public string GetKeyForProcessPersistence(Guid processId)
        {
            return $"{_providerNamespace}:processpersistence:{processId:N}";
        }

        public string GetKeyForProcessScheme(Guid schemeId)
        {
            return $"{_providerNamespace}:processscheme:{schemeId:N}";
        }

        public string GetKeyForCurrentScheme(string schemeCode, string parametersHash)
        {
            return $"{_providerNamespace}:currentscheme:{schemeCode}:{parametersHash}";
        }

        public string GetKeyForCurrentSchemes(string schemeCode)
        {
            return $"{_providerNamespace}:currentschemes:{schemeCode}";
        }

        public string GetKeySchemeHierarchy(Guid schemeId)
        {
            return $"{_providerNamespace}:schemehierarchy:{schemeId:N}";
        }

        public string GetKeyProcessRunning()
        {
            return $"{_providerNamespace}:processrunning";
        }

        public string GetKeyProcessStatus(Guid processId)
        {
            return $"{_providerNamespace}:processstatus:{processId:N}";
        }

        public string GetKeyProcessStatus(string processId)
        {
            return $"{_providerNamespace}:processstatus:{processId}";
        }

        public string GetKeyProcessStatusSetTime()
        {
            return $"{_providerNamespace}:processstatussettime";
        }

        public string GetKeyProcessTimer(Guid processId, string name)
        {
            return $"{_providerNamespace}:processtimer:{processId:N}:{name}";
        }

        public string GetKeyProcessTimers(Guid processId)
        {
            return $"{_providerNamespace}:processtimers:{processId:N}";
        }

        public string GetKeyTimerTime()
        {
            return $"{_providerNamespace}:timertime";
        }

        public string GetKeyTimer()
        {
            return $"{_providerNamespace}:timer";
        }

        public string GetKeyTimerIgnore()
        {
            return $"{_providerNamespace}:timerignore";
        }

        public string GetKeyTimerIgnoreLock(string timerId)
        {
            return $"{_providerNamespace}:timerignorelock:{timerId}";
        }

        public string GetKeyTimerIgnoreLock(Guid timerId)
        {
            return $"{_providerNamespace}:timerignorelock:{timerId:N}";
        }

        public string GetKeyGlobalParameter(string type)
        {
            return $"{_providerNamespace}:globalparameter:{type}";
        }

        public string GetKeyCanBeInlined()
        {
            return $"{_providerNamespace}:schemecanbeinlined";
        }

        public string GetKeyForInlined()
        {
            return $"{_providerNamespace}:schemeinlined";
        }

        public string GetKeyForTags()
        {
            return $"{_providerNamespace}:schemetags";
        }

        public string GetKeyForSubprocesses(Guid rootProcessId)
        {
            return $"{_providerNamespace}:subprocesses:{rootProcessId:N}";
        }

        public string GetKeyForRootProcess(Guid processId)
        {
            return $"{_providerNamespace}:subprocesses:{processId:N}";
        }

        public string GetKeyForWorkflowRuntimeStatuses()
        {
            return $"{_providerNamespace}:workflowruntimestatus";
        }

        public string GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus status)
        {
            return $"{_providerNamespace}:workflowruntimestatus:{status}";
        }

        public string GetKeyForWorkflowRuntimeLocks(string runtimeId)
        {
            return $"{_providerNamespace}:workflowruntimelocks:{runtimeId}";
        }

        public string GetKeyForWorkflowLastAliveSignals()
        {
            return $"{_providerNamespace}:workflowruntimelastalivesignals";
        }

        public string GetKeyForWorkflowRuntimeRestorers()
        {
            return $"{_providerNamespace}:workflowruntimerestorers";
        }

        public string GetKeyForWorkflowRuntimeProcesses(string runtimeId)
        {
            return $"{_providerNamespace}:workflowruntimeprocesses:{runtimeId}";
        }

        public string GetKeyForWorkflowProcessRuntimes()
        {
            return $"{_providerNamespace}:workflowprocessruntimes";
        }

        public string GetKeyForWorkflowSyncLocks(TimerCategory timerCategory)
        {
            return $"{_providerNamespace}:workflowsynclocks:{timerCategory}";
        }

        public string GetKeyForWorkflowRuntimeTimer(string timerName)
        {
            return $"{_providerNamespace}:workflowruntimetimers:{timerName}";
        }

        public string GetKeyInbox(Guid processId)
        {
            return $"{_providerNamespace}:inbox:{processId:N}";
        }

        public string GetKeyApprovalHistory(Guid documentId)
        {
            return $"{_providerNamespace}:approvalhistory:{documentId:N}";
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
            IDatabase db = _connector.GetDatabase();
            string hash = $"{processId:N}";
            bool exists = db.KeyExists(GetKeyProcessStatus(processId));

            if (!exists)
            {
                throw new StatusNotDefinedException();
            }

            string runtimeId = _runtime.Id;

            if (db.HashExists(GetKeyForWorkflowProcessRuntimes(), hash))
            {
                runtimeId = db.HashGet(GetKeyForWorkflowProcessRuntimes(), hash);
            }

            ITransaction tran = db.CreateTransaction();
            tran.AddCondition(Condition.StringNotEqual(GetKeyProcessStatus(processId), (int)ProcessStatus.Running.Id));
            tran.HashSetAsync(GetKeyProcessRunning(), hash, true);
            tran.StringSetAsync(GetKeyProcessStatus(processId), (int)ProcessStatus.Running.Id);
            tran.HashSetAsync(GetKeyProcessStatusSetTime(), hash, _runtime.RuntimeDateTimeNow.Ticks);

            if (runtimeId != _runtime.Id)
            {
                tran.SetRemoveAsync(GetKeyForWorkflowRuntimeProcesses(runtimeId), hash);
            }           

            tran.HashSetAsync(GetKeyForWorkflowProcessRuntimes(), hash, _runtime.Id);
            tran.SetAddAsync(GetKeyForWorkflowRuntimeProcesses(_runtime.Id), hash);

            bool res = tran.Execute();

            if (!res)
            {
                var status = db.StringGet(GetKeyProcessStatus(processId));

                _runtime.LogError("Failed to SetRunningStatus", new Dictionary<string, string>()
                {
                    { "Status", status.ToString() },
                    { "processId", processId.ToString()},
                    { "runtimeId", _runtime.Id }
                });
                

                throw new ImpossibleToSetStatusException();
            }
        }

        private void SetCustomStatus(Guid processId, ProcessStatus status, bool createIfnotDefined = false)
        {
            var db = _connector.GetDatabase();
            var hash = string.Format("{0:N}", processId);
            var exists = db.KeyExists(GetKeyProcessStatus(processId));
            if (!createIfnotDefined && !exists)
                throw new StatusNotDefinedException();
            var batch = db.CreateBatch();
            batch.HashDeleteAsync(GetKeyProcessRunning(), hash);
            batch.StringSetAsync(GetKeyProcessStatus(processId),(int)status.Id);
            batch.HashSetAsync(GetKeyProcessStatusSetTime(), hash, _runtime.RuntimeDateTimeNow.Ticks);

            if (db.HashExists(GetKeyForWorkflowProcessRuntimes(), hash))
            {
                RedisValue runtimeId = db.HashGet(GetKeyForWorkflowProcessRuntimes(), hash);

                if (runtimeId != _runtime.Id)
                {
                    batch.SetRemoveAsync(GetKeyForWorkflowRuntimeProcesses(runtimeId), hash);
                }
            }

            batch.HashSetAsync(GetKeyForWorkflowProcessRuntimes(), hash, _runtime.Id);

            if (!exists)
            {
                batch.SetAddAsync(GetKeyForWorkflowRuntimeProcesses(_runtime.Id), hash);
            }

            batch.Execute();
        }

        private void AddDeleteTimersOperationsToBatch(Guid processId, List<string> timersIgnoreList, IDatabase db, IBatch batch)
        {
            RedisValue[] timerNames = db.SetMembers(GetKeyProcessTimers(processId));

            var timers =
                timerNames.Select(x => new { Name = x, Value = db.StringGet(GetKeyProcessTimer(processId, x)) })
                    .Where(he => !timersIgnoreList.Contains(he.Name))
                    .Select(he => new TimerToExecute {Name = he.Name, ProcessId = processId, TimerId = Guid.Parse(he.Value)})
                    .ToList();

            foreach (TimerToExecute timer in timers)
            {
                batch.SortedSetRemoveAsync(GetKeyTimerTime(), timer.TimerId.ToString("N"));
                batch.KeyDeleteAsync(GetKeyProcessTimer(processId, timer.Name));
                batch.SetRemoveAsync(GetKeyProcessTimers(processId), timer.Name);
                batch.HashDeleteAsync(GetKeyTimerIgnore(), timer.TimerId.ToString("N"));
                batch.KeyDeleteAsync(GetKeyTimerIgnoreLock(timer.TimerId));
                batch.HashDeleteAsync(GetKeyTimer(), timer.TimerId.ToString("N"));
            }
        }

        private WorkflowRuntimeModel GetWorkflowRuntimeStatus(IDatabase db, string runtimeId)
        {
            RedisValue statusValue = db.HashGet(GetKeyForWorkflowRuntimeStatuses(), runtimeId);
            if (statusValue.HasValue)
            {
                var status = (RuntimeStatus)Enum.Parse(typeof(RuntimeStatus), statusValue);

                RedisValue lockValue = db.StringGet(GetKeyForWorkflowRuntimeLocks(runtimeId));

                DateTime? lastAliveSignal = default;

                double? lastAliveSignalValue = db.SortedSetScore(GetKeyForWorkflowLastAliveSignals(), runtimeId);

                if (lastAliveSignalValue.HasValue)
                {
                    lastAliveSignal = _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(lastAliveSignalValue.Value));
                }

                RedisValue restorerIdValue = db.HashGet(GetKeyForWorkflowRuntimeRestorers(), runtimeId);

                string sortedSetKey = GetKeyForWorkflowRuntimeTimer(TimerCategory.Timer.ToString());

                DateTime? nextTimerTime = default;

                double? nextTimerTimeValue = db.SortedSetScore(sortedSetKey, runtimeId);

                if (nextTimerTimeValue.HasValue)
                {
                    nextTimerTime = _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(nextTimerTimeValue.Value));
                }

                return new WorkflowRuntimeModel
                {
                    Lock = Guid.Parse(lockValue),
                    RuntimeId = runtimeId,
                    Status = status,
                    LastAliveSignal = lastAliveSignal,
                    RestorerId = restorerIdValue,
                    NextTimerTime = nextTimerTime
                };
            }

            return null;
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

            var key = GetKeyForCurrentScheme(processScheme.RootSchemeId.HasValue ? processScheme.RootSchemeCode : processScheme.SchemeCode,
                HashHelper.GenerateStringHash(processScheme.DefiningParameters));

            var isObsolete = !db.KeyExists(key);

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
                var isObsolete = !db.KeyExists(GetKeyForCurrentScheme(processScheme.RootSchemeCode, HashHelper.GenerateStringHash(parameters)));

                if (!ignoreObsolete && isObsolete)
                {
                    throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
                }

                return ConvertToSchemeDefinition(schemeId, isObsolete, processScheme);
            }
            else
            {
                var schemeIdValue = db.StringGet(GetKeyForCurrentScheme(schemeCode, HashHelper.GenerateStringHash(parameters)));

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
        public SchemeDefinition<XElement> SaveScheme(SchemeDefinition<XElement> scheme)
        {
            var db = _connector.GetDatabase();


            var tran = db.CreateTransaction();

            if (!scheme.IsObsolete) //there is only one current scheme can exists 
            {
                if (!scheme.RootSchemeId.HasValue)
                {
                    var key = GetKeyForCurrentScheme(scheme.SchemeCode, HashHelper.GenerateStringHash(scheme.DefiningParameters));
                    tran.AddCondition(Condition.KeyNotExists(key));
                    tran.StringSetAsync(key, String.Format("{0:N}", scheme.Id));
                    tran.SetAddAsync(GetKeyForCurrentSchemes(scheme.SchemeCode), key);
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

            return null;
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
            var key = GetKeyForCurrentScheme(schemeCode, HashHelper.GenerateStringHash(definingParameters));

            var batch = db.CreateBatch();
            batch.KeyDeleteAsync(key);
            batch.SetRemoveAsync(GetKeyForCurrentSchemes(schemeCode), key);

            batch.Execute();
        }

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        public void SetSchemeIsObsolete(string schemeCode)
        {
            IDatabase db = _connector.GetDatabase();

            RedisValue[] keys = db.SetMembers(GetKeyForCurrentSchemes(schemeCode));

            var batch = db.CreateBatch();

            foreach(var k in keys.Where(k => k.HasValue))
            {
                batch.KeyDeleteAsync(k.ToString());
            }
            batch.KeyDeleteAsync(GetKeyForCurrentSchemes(schemeCode));
        }


        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="inlinedSchemes">Scheme codes to be inlined into this scheme</param>
        /// <param name="scheme">Not parsed scheme</param>
        /// <param name="canBeInlined">if true - this scheme can be inlined into another schemes</param>
        public void SaveScheme(string schemeCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags)
        {
            var db = _connector.GetDatabase();
            var tran = db.CreateTransaction();
            tran.StringSetAsync(GetKeyForScheme(schemeCode), scheme);

            var keyForInlined = GetKeyForInlined();
            var keyCanBeInlined = GetKeyCanBeInlined();

            if (inlinedSchemes == null || !inlinedSchemes.Any())
            {
                tran.HashDeleteAsync(keyForInlined, schemeCode);
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

            string keyTags = GetKeyForTags();

            tran.HashSetAsync(keyTags, schemeCode, TagHelper.ToTagStringForDatabase(tags));

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

            foreach (HashEntry pair in pairs)
            {
                var inlined = JsonConvert.DeserializeObject<List<string>>(pair.Value.ToString());
                if (inlined.Contains(schemeCode))
                    res.Add(pair.Name.ToString());
            }

            return res;
        }

        public List<string> SearchSchemesByTags(params string[] tags)
        {
            return SearchSchemesByTags(tags?.AsEnumerable());
        }

        public List<string> SearchSchemesByTags(IEnumerable<string> tags)
        {
            bool isEmpty = tags == null || !tags.Any();

            IDatabase db = _connector.GetDatabase();

            HashEntry[] pairs = db.HashGetAll(GetKeyForTags());

            var res = new List<string>();

            foreach (HashEntry pair in pairs)
            {
                if (String.IsNullOrWhiteSpace(pair.Value))
                {
                    continue;
                }

                if (isEmpty)
                {
                    res.Add(pair.Name.ToString());
                }
                else
                {
                    List<string> storedTags = TagHelper.FromTagStringForDatabase(pair.Value);

                    if (storedTags.Any(st => tags.Contains(st)))
                    {
                        res.Add(pair.Name.ToString());
                    }
                }
            }

            return res;
        }

        public void AddSchemeTags(string schemeCode, params string[] tags)
        {
            AddSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public void AddSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            UpdateTags(schemeCode, (schemeTags) => tags.Concat(schemeTags).ToList());
        }

        public void RemoveSchemeTags(string schemeCode, params string[] tags)
        {
            RemoveSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public void RemoveSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            UpdateTags(schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList());
        }

        public void SetSchemeTags(string schemeCode, params string[] tags)
        {
            SetSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public void SetSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            UpdateTags(schemeCode, (schemeTags) => tags.ToList());
        }

        private void UpdateTags(string schemeCode, Func<List<string>, List<string>> getNewTags)
        {
            IDatabase db = _connector.GetDatabase();
            string key = GetKeyForTags();
            ITransaction tran = db.CreateTransaction();

            RedisValue dbValue = db.HashGet(key, schemeCode);

            string dbTags = "";

            if (dbValue.HasValue)
            {
                dbTags = dbValue.ToString();
            }

            var newTags = getNewTags(TagHelper.FromTagStringForDatabase(dbTags));

            tran.HashSetAsync(key, schemeCode, TagHelper.ToTagStringForDatabase(newTags));

            string scheme = db.StringGet(GetKeyForScheme(schemeCode));
            scheme = _runtime.Builder.ReplaceTagsInScheme(scheme, newTags);
            tran.StringSetAsync(GetKeyForScheme(schemeCode), scheme);

            bool res = tran.Execute();

            if (!res)
            {
                throw new ImpossibleToUpdateSchemeTagsException(schemeCode,
                    "Transaction failed (may be because of concurrency)");
            }
        }

        #endregion

        #region Implementation of IPersistenceProvider

        public void DeleteInactiveTimersByProcessId(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            string ignoreKey = GetKeyTimerIgnore();
            IBatch batch = db.CreateBatch();

            RedisValue[] timerNames = db.SetMembers(GetKeyProcessTimers(processId));

            var timers =
                timerNames.Select(x => new { Name = x, Value = db.StringGet(GetKeyProcessTimer(processId, x)) })
                    .Where(he => db.HashExists(ignoreKey, he.Value))
                    .Select(he => new TimerToExecute {Name = he.Name, ProcessId = processId, TimerId = Guid.Parse(he.Value)})
                    .ToList();

            foreach (TimerToExecute timer in timers)
            {
                batch.SortedSetRemoveAsync(GetKeyTimerTime(), timer.TimerId.ToString("N"));
                batch.KeyDeleteAsync(GetKeyProcessTimer(processId, timer.Name));
                batch.SetRemoveAsync(GetKeyProcessTimers(processId), timer.Name);
                batch.HashDeleteAsync(ignoreKey, timer.TimerId.ToString("N"));
                batch.HashDeleteAsync(GetKeyTimer(), timer.TimerId.ToString("N"));
            }

            batch.Execute();
        }

        public async Task DeleteTimerAsync(Guid timerId)
        {
            IDatabase db = _connector.GetDatabase();

            string timerKey = timerId.ToString("N");

            RedisValue timerValue = await db.HashGetAsync(GetKeyTimer(), timerKey).ConfigureAwait(false);

            if (timerValue.HasValue)
            {
                TimerToExecute timer = JsonConvert.DeserializeObject<TimerToExecute>(timerValue);

                IBatch batch = db.CreateBatch();

#pragma warning disable 4014
                batch.SortedSetRemoveAsync(GetKeyTimerTime(), timerKey);
                batch.KeyDeleteAsync(GetKeyProcessTimer(timer.ProcessId, timer.Name));
                batch.SetRemoveAsync(GetKeyProcessTimers(timer.ProcessId), timer.Name);
                batch.HashDeleteAsync(GetKeyTimerIgnore(), timerKey);
                batch.KeyDeleteAsync(GetKeyTimerIgnoreLock(timerKey));
                batch.HashDeleteAsync(GetKeyTimer(), timerKey);
#pragma warning restore 4014

                batch.Execute();
            }
        }

        public List<Guid> GetRunningProcesses(string runtimeId = null)
        {
            IDatabase db = _connector.GetDatabase();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                return db.SetMembers(GetKeyForWorkflowRuntimeProcesses(runtimeId)).Where(rp => rp.HasValue)
                    .Select(x => Guid.Parse(x)).ToList();
            }

            return db.HashKeys(GetKeyProcessRunning()).Where(rp => rp.HasValue).Select(x => Guid.Parse(x)).ToList();
        }

        public WorkflowRuntimeModel CreateWorkflowRuntime(string runtimeId, RuntimeStatus status)
        {
            IDatabase db = _connector.GetDatabase();

            IBatch batch = db.CreateBatch();

            var runtime = new WorkflowRuntimeModel() {RuntimeId = runtimeId, Lock = Guid.NewGuid(), Status = status};

            batch.HashSetAsync(GetKeyForWorkflowRuntimeStatuses(), runtimeId, status.ToString());
            batch.StringSetAsync(GetKeyForWorkflowRuntimeLocks(runtimeId), runtime.Lock.ToString("N"));
            batch.SetAddAsync(GetKeyForWorkflowRuntimeStatusSet(status), runtimeId);

            batch.Execute();

            return runtime;
        }

        public void DeleteWorkflowRuntime(string name)
        {
            IDatabase db = _connector.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            RedisValue statusValue = db.HashGet(GetKeyForWorkflowRuntimeStatuses(), name);

            if (statusValue.HasValue)
            {
                var status = (RuntimeStatus)Enum.Parse(typeof(RuntimeStatus), statusValue);

                tran.HashDeleteAsync(GetKeyForWorkflowRuntimeStatuses(), name);
                tran.KeyDeleteAsync(GetKeyForWorkflowRuntimeLocks(name));
                tran.SetRemoveAsync(GetKeyForWorkflowRuntimeStatusSet(status), name);
                tran.SortedSetRemoveAsync(GetKeyForWorkflowLastAliveSignals(), name);
                tran.HashDeleteAsync(GetKeyForWorkflowRuntimeRestorers(), name);

                tran.SortedSetRemoveAsync(GetKeyForWorkflowRuntimeTimer(TimerCategory.Timer.ToString()), name);
                tran.SortedSetRemoveAsync(GetKeyForWorkflowRuntimeTimer(TimerCategory.ServiceTimer.ToString()), name);

                tran.KeyDeleteAsync(GetKeyForWorkflowRuntimeProcesses(name));
            }

            tran.Execute();
        }

        public WorkflowRuntimeModel UpdateWorkflowRuntimeStatus(WorkflowRuntimeModel runtime, RuntimeStatus status)
        {
            IDatabase db = _connector.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            Guid oldLock = runtime.Lock;
            runtime.Lock = Guid.NewGuid();
            runtime.Status = status;

            tran.AddCondition(Condition.StringEqual(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), oldLock.ToString("N")));

            RedisValue statusValue = db.HashGet(GetKeyForWorkflowRuntimeStatuses(), runtime.RuntimeId);
            if (statusValue.HasValue)
            {
                var currentStatus = (RuntimeStatus)Enum.Parse(typeof(RuntimeStatus), statusValue);
                tran.SetRemoveAsync(GetKeyForWorkflowRuntimeStatusSet(currentStatus), runtime.RuntimeId);
            }

            tran.HashSetAsync(GetKeyForWorkflowRuntimeStatuses(), runtime.RuntimeId, status.ToString());
            tran.StringSetAsync(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), runtime.Lock.ToString("N"));
            tran.SetAddAsync(GetKeyForWorkflowRuntimeStatusSet(status), runtime.RuntimeId);

            bool res = tran.Execute();

            if (!res)
            {
                throw new ImpossibleToSetRuntimeStatusException();
            }

            return runtime;
        }

        public (bool Success, WorkflowRuntimeModel UpdatedModel) UpdateWorkflowRuntimeRestorer(WorkflowRuntimeModel runtime, string restorerId)
        {
            IDatabase db = _connector.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            Guid oldLock = runtime.Lock;
            runtime.Lock = Guid.NewGuid();
            runtime.RestorerId = restorerId;

            tran.AddCondition(Condition.StringEqual(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), oldLock.ToString("N")));

            tran.HashSetAsync(GetKeyForWorkflowRuntimeRestorers(), runtime.RuntimeId, runtime.RestorerId);
            tran.StringSetAsync(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), runtime.Lock.ToString("N"));

            bool res = tran.Execute();

            if (!res)
            {
                return (false, null);
            }

            return (true, runtime);
        }

        public WorkflowRuntimeModel GetWorkflowRuntimeModel(string runtimeId)
        {
            IDatabase db = _connector.GetDatabase();
            return GetWorkflowRuntimeStatus(db, runtimeId);
        }

        public bool MultiServerRuntimesExist()
        {
            IDatabase db = _connector.GetDatabase();
            string runtimesKey = GetKeyForWorkflowRuntimeStatuses();

            int emptyExists = db.HashExists(runtimesKey, Guid.Empty.ToString()) ? 1 : 0;

            if (db.HashLength(runtimesKey) > emptyExists)
            {
                return true;
            }

            foreach (RuntimeStatus status in Enum.GetValues(typeof(RuntimeStatus)))
            {
                if (status == RuntimeStatus.Single || status == RuntimeStatus.Terminated || status == RuntimeStatus.Dead)
                {
                    continue;
                }

                if (db.SetLength(GetKeyForWorkflowRuntimeStatusSet(status)) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public int SendRuntimeLastAliveSignal()
        {
            IDatabase db = _connector.GetDatabase();
            string runtimeId = _runtime.Id;

            if (!db.SetContains(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive), runtimeId) &&
                !db.SetContains(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.SelfRestore), runtimeId))
            {
                return 0;
            }

            double unixTime = (_runtime.RuntimeDateTimeNow.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

            db.SortedSetAdd(GetKeyForWorkflowLastAliveSignals(), runtimeId, unixTime);

            return 1;
        }

        public int SingleServerRuntimesCount()
        {
            IDatabase db = _connector.GetDatabase();
            return db.SetContains(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Single), Guid.Empty.ToString()) ? 1 : 0;
        }

        public int ActiveMultiServerRuntimesCount(string currentRuntimeId)
        {
            IDatabase db = _connector.GetDatabase();

            int result = (int)(
                db.SetLength(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive)) +
                db.SetLength(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Restore)) +
                db.SetLength(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.SelfRestore))
            );

            if (db.SetContains(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive), currentRuntimeId))
            {
                result -= 1;
            }
            else if (db.SetContains(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Restore), currentRuntimeId))
            {
                result -= 1;
            }
            else if (db.SetContains(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.SelfRestore), currentRuntimeId))
            {
                result -= 1;
            }

            return result;
        }

        /// <summary>
        /// Init the provider
        /// </summary>
        /// <param name="runtime">Workflow runtime instance which owned the provider</param>
        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
            CheckInitialData();
        }

        private void CheckInitialData()
        {
            IDatabase db = _connector.GetDatabase();

            if (!db.KeyExists(GetKeyForWorkflowSyncLocks(TimerCategory.Timer)))
            {
                db.StringSetAsync(GetKeyForWorkflowSyncLocks(TimerCategory.Timer), Guid.NewGuid().ToString("N"));
            }

            if (!db.KeyExists(GetKeyForWorkflowSyncLocks(TimerCategory.ServiceTimer)))
            {
                db.StringSetAsync(GetKeyForWorkflowSyncLocks(TimerCategory.ServiceTimer), Guid.NewGuid().ToString("N"));
            }

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
                ParentProcessId = processInstance.ParentProcessId,
                TenantId = processInstance.TenantId
            };

            var processKey = GetKeyForProcessInstance(processInstance.ProcessId);

            var db = _connector.GetDatabase();
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyNotExists(processKey));
            tran.StringSetAsync(processKey, JsonConvert.SerializeObject(newProcess));
            if (processInstance.IsSubprocess)
            {
                tran.StringSetAsync(GetKeyForRootProcess(processInstance.ProcessId), processInstance.RootProcessId.ToString("N"));
                tran.ListLeftPushAsync(GetKeyForSubprocesses(processInstance.RootProcessId), processInstance.ProcessId.ToString("N"));
            }

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
                    parameters.Add(ParameterDefinition.Create(parameterDefinition,
                        ParametersSerializer.Deserialize(persistedParameter.Value, parameterDefinition.Type)));
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
                    workflowProcessInstance.RootProcessId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterTenantId.Name),
                    workflowProcessInstance.TenantId)
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
                        return (string)ptp.Value;
                    return ParametersSerializer.Serialize(ptp.Value, ptp.Type);

                });

            var parametersToRemove =
                processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Value == null).Select(ptp => ptp.Name)
                    .ToList();

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
                SetCustomStatus(processId, newStatus);
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
        public void SetWorkflowTerminated(ProcessInstance processInstance)
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

            var now = _runtime.RuntimeDateTimeNow.Ticks;

            var batch = db.CreateBatch();

            foreach (var hash in allRunningIds.Where(ari => ari.HasValue))
            {
                batch.HashSetAsync(GetKeyProcessStatusSetTime(), hash, now);
                batch.StringSetAsync(GetKeyProcessStatus(hash), (int)ProcessStatus.Idled.Id);
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
            var identityId = paramIdentityId == null ? string.Empty : (string)paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string)paramImpIdentityId.Value;

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

            var batch = db.CreateBatch();

            batch.StringSetAsync(key, JsonConvert.SerializeObject(inst));

            if (WriteToHistory)
            {
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

                batch.ListRightPushAsync(
                    GetKeyForProcessHistory((WriteSubProcessToRoot && processInstance.IsSubprocess)
                        ? processInstance.RootProcessId
                        : processInstance.ProcessId), JsonConvert.SerializeObject(history));
            }

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
            var statusValue = db.StringGet(GetKeyProcessStatus(processId));
            if (!statusValue.HasValue)
                return ProcessStatus.NotFound;
            var statusId = Int32.Parse(statusValue);
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


        public void RegisterTimer(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            var db = _connector.GetDatabase();
            var unixTime = (nextExecutionDateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;


            var timerId = Guid.NewGuid();
            var timerToExecute = new TimerToExecute() {ProcessId = processId, RootProcessId = rootProcessId, Name = name, TimerId = timerId};

            var tran = db.CreateTransaction();

            if (notOverrideIfExists)
            {
                tran.AddCondition(Condition.KeyNotExists(GetKeyProcessTimer(processId, name)));
            }

            tran.SortedSetAddAsync(GetKeyTimerTime(), timerId.ToString("N"), unixTime);
            tran.StringSetAsync(GetKeyProcessTimer(processId, name), timerId.ToString("N"));
            tran.SetAddAsync(GetKeyProcessTimers(processId), name);
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
        /// Clears sign Ignore for specific timers
        /// </summary>
        public void ClearTimerIgnore(Guid timerId)
        {
            var db = _connector.GetDatabase();
            var keyTimerIgnore = GetKeyTimerIgnore();
            db.HashDelete(keyTimerIgnore, timerId.ToString("N"));
            db.KeyDelete(GetKeyTimerIgnoreLock(timerId));
        }

        public int SetTimerIgnore(Guid timerId)
        {
            var db = _connector.GetDatabase();
            var keyTimerIgnore = GetKeyTimerIgnore();
            var timerIdValue = timerId.ToString("N");

            var tran = db.CreateTransaction();
            var conditionResult = tran.AddCondition(Condition.KeyNotExists(GetKeyTimerIgnoreLock(timerIdValue)));
            tran.HashSetAsync(keyTimerIgnore, timerIdValue, true);
            tran.StringSetAsync(GetKeyTimerIgnoreLock(timerIdValue), String.Empty);
            tran.Execute();

            return conditionResult.WasSatisfied ? 1 : 0;
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
                batchTimerNotExists.KeyDeleteAsync(GetKeyTimerIgnoreLock(timerId));
                batchTimerNotExists.Execute();
                return;
            }

            var timer = JsonConvert.DeserializeObject<TimerToExecute>(timerValue);
            var keyProcessTimer = GetKeyProcessTimer(timer.ProcessId, timer.Name);
            var timerInProcessIdValue = db.StringGet(keyProcessTimer);

            var batch = db.CreateBatch();

            batch.SortedSetRemoveAsync(keyTimerTime, timerId.ToString("N"));

            if (timerInProcessIdValue.HasValue && Guid.Parse(timerInProcessIdValue) == timerId)
                batch.HashDeleteAsync(keyProcessTimer, timer.Name);
            batch.HashDeleteAsync(keyTimerIgnore, timerId.ToString("N"));
            batch.HashDeleteAsync(keyTimer, timerId.ToString("N"));
            batch.KeyDeleteAsync(GetKeyTimerIgnoreLock(timerId));
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
                var closestTimers =
                    db.SortedSetRangeByScore(keyTimer, double.NegativeInfinity, double.PositiveInfinity, Exclude.None, Order.Ascending, skip, 10);
                if (!closestTimers.Any())
                    return null;

                foreach (var closestTimer in closestTimers)
                {
                    if (!db.HashExists(keyTimerIgnore, closestTimer))
                    {
                        double? unixTime = db.SortedSetScore(keyTimer, closestTimer);
                        if (unixTime.HasValue)
                        {
                            return _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(unixTime.Value));
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
                tran.AddCondition(Condition.KeyNotExists(GetKeyTimerIgnoreLock(timerIdValue)));
                tran.StringSetAsync(GetKeyTimerIgnoreLock(timerIdValue), String.Empty);
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

        public List<Core.Model.WorkflowTimer> GetTopTimersToExecute(int top)
        {
            double unixTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            IDatabase db = _connector.GetDatabase();

            var timerIds = db.SortedSetRangeByScoreWithScores(GetKeyTimerTime(), double.NegativeInfinity, unixTime, take: top)
                .Where(rv => rv.Element.HasValue).ToList();

            var result = new List<Core.Model.WorkflowTimer>();

            if (!timerIds.Any())
            {
                return result;
            }

            foreach (SortedSetEntry timerIdValue in timerIds)
            {
                RedisValue timerValue = db.HashGet(GetKeyTimer(), timerIdValue.Element);
                if (timerValue.HasValue)
                {
                    TimerToExecute t = JsonConvert.DeserializeObject<TimerToExecute>(timerValue);

                    result.Add(new Core.Model.WorkflowTimer
                    {
                        Name = t.Name,
                        ProcessId = t.ProcessId,
                        TimerId = t.TimerId,
                        NextExecutionDateTime = _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(timerIdValue.Score)),
                        RootProcessId = t.RootProcessId
                    });
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
            IDatabase db = _connector.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            AddDeleteTimersOperationsToBatch(processId, new List<string>(), db, tran);
            tran.KeyDeleteAsync(GetKeyForProcessInstance(processId));
            tran.KeyDeleteAsync(GetKeyForProcessPersistence(processId));
            tran.KeyDeleteAsync(GetKeyForProcessHistory(processId));
            string hash = $"{processId:N}";
            tran.KeyDeleteAsync(GetKeyProcessStatus(hash));
            tran.HashDeleteAsync(GetKeyProcessRunning(), hash);
            tran.HashDeleteAsync(GetKeyProcessStatusSetTime(), hash);
            
            RedisValue[] timerNames = db.SetMembers(GetKeyProcessTimers(processId));

            foreach(RedisValue tn in timerNames)
            {
                tran.KeyDeleteAsync(GetKeyProcessTimer(processId, tn));
            }
            tran.KeyDeleteAsync(GetKeyProcessTimers(processId));

            RedisValue runtimeId = db.HashGet(GetKeyForWorkflowProcessRuntimes(), hash);

            tran.SetRemoveAsync(GetKeyForWorkflowRuntimeProcesses(runtimeId), hash);
            tran.HashDeleteAsync(GetKeyForWorkflowProcessRuntimes(), hash);

            RedisValue rootProcessId = db.StringGet(GetKeyForRootProcess(processId));

            if (rootProcessId.HasValue)
            {
                tran.ListRemoveAsync(GetKeyForSubprocesses(new Guid(rootProcessId.ToString())), processId.ToString("N"));
                tran.KeyDeleteAsync(GetKeyForRootProcess(processId));
            }

            tran.Execute();
        }

        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processIds">List of ids of the process</param>
        public void DeleteProcess(Guid[] processIds)
        {
            foreach (Guid processId in processIds)
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
            return GetTimersForProcess(processId, false);
        }

        public IEnumerable<ProcessTimer> GetActiveTimersForProcess(Guid processId)
        {
            return GetTimersForProcess(processId, true);
        }

        private IEnumerable<ProcessTimer> GetTimersForProcess(Guid processId, bool excludeIgnored)
        {
            IDatabase db = _connector.GetDatabase();

            RedisValue[] timerNames = db.SetMembers(GetKeyProcessTimers(processId));

            var timerIds = timerNames.Select(x => new { Name = x, Id = Guid.Parse(db.StringGet(GetKeyProcessTimer(processId, x))) });

            var result = new List<ProcessTimer>(timerIds.Count());

            var times = db.SortedSetRangeByRankWithScores(GetKeyTimerTime())
                .Where(rv => rv.Element.HasValue)
                .Select(t => new {Id = Guid.Parse(t.Element), Time = t.Score});

            if (excludeIgnored)
            {
                string keyTimerIgnore = GetKeyTimerIgnore();
                timerIds = timerIds.Where(t => !db.HashExists(keyTimerIgnore, t.Id.ToString("N")));
            }

            var xxxx = timerIds.ToList();
            var yyyy = times.ToList();

            return timerIds.Select(x =>
                new ProcessTimer(x.Id, x.Name, _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(times.First(t => t.Id == x.Id).Time))));
        }

        public DateTime? GetNextTimerDate(TimerCategory timerCategory, int timerInterval)
        {
            string timerCategoryName = timerCategory.ToString();

            IDatabase db = _connector.GetDatabase();

            RedisValue lockValue = db.StringGet(GetKeyForWorkflowSyncLocks(timerCategory));

            if (!lockValue.HasValue)
            {
                throw new Exception($"Sync lock {timerCategoryName} not found");
            }

            string sortedSetKey = GetKeyForWorkflowRuntimeTimer(timerCategoryName);

            DateTime result = _runtime.RuntimeDateTimeNow;

            int index = 0;

            while (true)
            {
                SortedSetEntry[] se = db.SortedSetRangeByRankWithScores(sortedSetKey, index, index + 1, Order.Descending);

                index += 1;

                if (se.Length == 0)
                {
                    break;
                }

                string runtimeId = se.First().Element;

                if (runtimeId == _runtime.Id)
                {
                    continue;
                }

                if (db.SetContains(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive), runtimeId))
                {
                    DateTime newTime = _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(se.First().Score));

                    if (newTime > result)
                    {
                        result = newTime;
                    }

                    break;
                }
            }

            result += TimeSpan.FromMilliseconds(timerInterval);

            var newLock = Guid.NewGuid();

            ITransaction tran = db.CreateTransaction();

            tran.AddCondition(Condition.StringEqual(GetKeyForWorkflowSyncLocks(timerCategory), lockValue));
            tran.SortedSetAddAsync(sortedSetKey, _runtime.Id, (result.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds);
            tran.StringSetAsync(GetKeyForWorkflowSyncLocks(timerCategory), newLock.ToString("N"));

            bool tranResult = tran.Execute();

            if (!tranResult)
            {
                return null;
            }

            return result;
        }

        public List<WorkflowRuntimeModel> GetWorkflowRuntimes()
        {
            IDatabase db = _connector.GetDatabase();
            return db.HashKeys(GetKeyForWorkflowRuntimeStatuses()).Select(v => GetWorkflowRuntimeStatus(db, v.ToString())).ToList();
        }

        public async Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId)
        {
            var db = _connector.GetDatabase();

            var processIdsLists = db.ListRange(GetKeyForSubprocesses(rootProcessId)).Where(v => v.HasValue).Select(v => new Guid(v.ToString())).ToList();

            var processInfo = processIdsLists
                .Select(processId => db.StringGet(GetKeyForProcessInstance(processId)))
                .Where(v => v.HasValue)
                .Select(v =>
                {
                    WorkflowProcessInstance processInstance = JsonConvert.DeserializeObject<WorkflowProcessInstance>(v);

                    if (!processInstance.SchemeId.HasValue)
                    {
                        throw SchemeNotFoundException.Create(processInstance.Id, SchemeLocation.WorkflowProcessInstance);
                    }

                    return (processId: processInstance.Id, schemeId: processInstance.SchemeId.Value, parentProcessId: processInstance.ParentProcessId,
                        rootProcessId: processInstance.RootProcessId);
                }).ToList();


            var startingTransitions = processInfo.Select(i => i.schemeId).Distinct().Select(schemeId =>
            {
                var s = db.StringGet(GetKeyForProcessScheme(schemeId));
                if (!s.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
                }

                var processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(s);

                return (schemeId: schemeId, startingTransition: processScheme.StartingTransition);
            }).ToDictionary(t => t.schemeId, t => t.startingTransition);


            return ProcessInstanceTreeItem.Create(rootProcessId, processInfo, startingTransitions);
        }

        public IApprovalProvider GetIApprovalProvider()
        {
            return this;
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

        #region IApprovalProvider

        public async Task DropWorkflowInboxAsync(Guid processId)
        {
            var db = _connector.GetDatabase();
            db.KeyDelete(GetKeyInbox(processId));
        }

        public async Task InsertInboxAsync(Guid processId, List<string> newActors)
        {
            var db = _connector.GetDatabase();
            var inboxItems = newActors.Select(newactor => new WorkflowInbox() {IdentityId = newactor, ProcessId = processId}).ToArray();
            var batch = db.CreateBatch();
            foreach (var inboxItem in inboxItems)
            {
#pragma warning disable 4014
                batch.ListRightPushAsync(GetKeyInbox(processId), JsonConvert.SerializeObject(inboxItem));
#pragma warning restore 4014
            }

            batch.Execute();
        }

        public async Task WriteApprovalHistoryAsync(Guid documentId, string currentState, string nextState, string triggerName, string allowedToEmployeeNames,
            long order)
        {
            var db = _connector.GetDatabase();
            var batch = db.CreateBatch();
            var historyItem = new WorkflowApprovalHistory
            {
                AllowedTo = allowedToEmployeeNames,
                DestinationState = nextState,
                ProcessId = documentId,
                InitialState = currentState,
                TriggerName = triggerName,
                Sort = order
            };
#pragma warning disable 4014
            batch.ListRightPushAsync(GetKeyApprovalHistory(documentId), JsonConvert.SerializeObject(historyItem));
#pragma warning restore 4014
            batch.Execute();
        }

        public async Task UpdateApprovalHistoryAsync(Guid documentId, string currentState, string nextState, string triggerName, string identityId, long order,
            string comment)
        {
            var db = _connector.GetDatabase();
            var key = GetKeyApprovalHistory(documentId);
            var items = db.ListRange(key).Select(x => JsonConvert.DeserializeObject<WorkflowApprovalHistory>(x)).ToList();
            var historyItem = items.FirstOrDefault(h => h.ProcessId == documentId && !h.TransitionTime.HasValue &&
                                                        h.InitialState == currentState && h.DestinationState == nextState);
            var batch = db.CreateBatch();

            if (historyItem == null)
            {
                historyItem = new WorkflowApprovalHistory
                {
                    AllowedTo = string.Empty,
                    DestinationState = nextState,
                    ProcessId = documentId,
                    InitialState = currentState,
                    Sort = order,
                    TriggerName = triggerName,
                    Commentary = comment,
                    TransitionTime = _runtime.RuntimeDateTimeNow,
                    IdentityId = identityId
                };
                items.Add(historyItem);
            }
            else
            {
                historyItem.TriggerName = triggerName;
                historyItem.TransitionTime = _runtime.RuntimeDateTimeNow;
                historyItem.IdentityId = identityId;
                historyItem.Commentary = comment;
            }

            db.KeyDelete(key);

            foreach (var record in items)
            {
#pragma warning disable 4014
                batch.ListRightPushAsync(key, JsonConvert.SerializeObject(record));
#pragma warning restore 4014
            }

            batch.Execute();
        }

        public async Task DropEmptyApprovalHistoryAsync(Guid processId)
        {
            var db = _connector.GetDatabase();
            var key = GetKeyApprovalHistory(processId);
            var items = db.ListRange(key).Select(x => JsonConvert.DeserializeObject<WorkflowApprovalHistory>(x)).ToList();
            var notEmpty = items.Where(x => x.TransitionTime.HasValue);
            var batch = db.CreateBatch();
            db.KeyDelete(key);

            foreach (var record in notEmpty)
            {
#pragma warning disable 4014
                batch.ListRightPushAsync(key, JsonConvert.SerializeObject(record));
#pragma warning restore 4014
            }

            batch.Execute();
        }

        #endregion IApprovalProvider
    }
}
