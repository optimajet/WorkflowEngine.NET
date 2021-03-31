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
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Runtime.Timers;
using StackExchange.Redis;

namespace OptimaJet.Workflow.Redis
{
    public class RedisProvider : IWorkflowProvider
    {
        private readonly ConnectionMultiplexer _connector;
        private readonly string _providerNamespace;
        private WorkflowRuntime _runtime;
        private readonly bool _writeToHistory;
        private readonly bool _writeSubProcessToRoot;

        public RedisProvider(ConnectionMultiplexer connector, string providerNamespace = "wfe", bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            _connector = connector;
#if !NETCOREAPP
            if (_connector != null) _connector.PreserveAsyncOrder = false;
#endif
            _providerNamespace = providerNamespace;
            _writeToHistory = writeToHistory;
            _writeSubProcessToRoot = writeSubProcessToRoot;
        }

        #region Implementation of IWorkflowGenerator<out XElement>

        /// <summary>
        /// Generate not parsed process scheme
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="parameters">Parameters for creating scheme</param>
        /// <returns>Not parsed process scheme</returns>
       public virtual async Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
            {
                throw new InvalidOperationException("Parameters not supported");
            }

            return await GetSchemeAsync(schemeCode).ConfigureAwait(false);
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
        public string GetKeyForProcessHistory()
        {
            return $"{_providerNamespace}:processhistory";
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
            return $"{_providerNamespace}:rootprocess:{processId:N}";
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
        
        /// HashSet off <see cref="Guid"/> and <see cref="WorkflowInbox"/>
        public string GetKeyInbox()
        {
            return $"{_providerNamespace}:inbox"; //List
        }
        
        /// HashSet off <see cref="Guid"/> and <see cref="string"/>
        public string GetKeyInboxByProcessId(Guid processId)
        {
            return $"{_providerNamespace}:inbox:processid:{processId:N}";
        }
        
        /// HashSet off <see cref="Guid"/> and <see cref="Guid"/>
        public string GetKeyInboxByIdentityId(string identityId)
        {
            return $"{_providerNamespace}:inbox:identityid:{identityId:N}";
        }
        
        /// HashSet off <see cref="Guid"/> and <see cref="WorkflowApprovalHistory"/>
        public string GetKeyApprovalHistory()
        {
            return $"{_providerNamespace}:approvalhistory";
        }
        
        /// HashSet off <see cref="Guid"/> and <see cref="string"/>
        public string GetKeyApprovalHistoryByProcessId(Guid processId)
        {
            return $"{_providerNamespace}:approvalhistory:processid:{processId:N}";
        }
        /// HashSet off <see cref="Guid"/> and <see cref="Guid"/>
        public string GetKeyApprovalHistoryByIdentityId(string identityId)
        {
            return $"{_providerNamespace}:approvalhistory:identityid:{identityId:N}";
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

        private async Task SetRunningStatusAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            string hash = $"{processId:N}";
            bool exists = await db.KeyExistsAsync(GetKeyProcessStatus(processId)).ConfigureAwait(false);

            if (!exists)
            {
                throw new StatusNotDefinedException();
            }

            string runtimeId = _runtime.Id;

            if (await db.HashExistsAsync(GetKeyForWorkflowProcessRuntimes(), hash).ConfigureAwait(false))
            {
                runtimeId = await db.HashGetAsync(GetKeyForWorkflowProcessRuntimes(), hash).ConfigureAwait(false);
            }

            ITransaction tran = db.CreateTransaction();
            tran.AddCondition(Condition.StringNotEqual(GetKeyProcessStatus(processId), (int)ProcessStatus.Running.Id));
#pragma warning disable 4014
            tran.HashSetAsync(GetKeyProcessRunning(), hash, true);
            tran.StringSetAsync(GetKeyProcessStatus(processId), (int)ProcessStatus.Running.Id);
            tran.HashSetAsync(GetKeyProcessStatusSetTime(), hash, _runtime.RuntimeDateTimeNow.Ticks);

            if (runtimeId != _runtime.Id)
            {
                tran.SetRemoveAsync(GetKeyForWorkflowRuntimeProcesses(runtimeId), hash);
            }           

            tran.HashSetAsync(GetKeyForWorkflowProcessRuntimes(), hash, _runtime.Id);
            tran.SetAddAsync(GetKeyForWorkflowRuntimeProcesses(_runtime.Id), hash);
#pragma warning restore 4014

            bool res = await tran.ExecuteAsync().ConfigureAwait(false);

            if (!res)
            {
                RedisValue status = await db.StringGetAsync(GetKeyProcessStatus(processId)).ConfigureAwait(false);

                _runtime.LogErrorIfLoggerExists("Failed to SetRunningStatus", new Dictionary<string, string>()
                {
                    { "Status",status.HasValue ? status.ToString() : ProcessStatus.NotFound.Name },
                    { "processId", processId.ToString()},
                    { "runtimeId", _runtime.Id }
                });

                if (!status.HasValue)
                {
                    throw new StatusNotDefinedException();
                }
                

                throw new ImpossibleToSetStatusException();
            }
        }

        private async Task SetCustomStatusAsync(Guid processId, ProcessStatus status, bool createIfNotDefined = false)
        {
            IDatabase db = _connector.GetDatabase();
            string hash = $"{processId:N}";
            bool exists = await db.KeyExistsAsync(GetKeyProcessStatus(processId)).ConfigureAwait(false);
            if (!createIfNotDefined && !exists)
            {
                throw new StatusNotDefinedException();
            }

            IBatch batch = db.CreateBatch();
            // ReSharper disable once UseObjectOrCollectionInitializer
            var batchTasks = new List<Task>();
#pragma warning disable 4014
            batchTasks.Add(batch.HashDeleteAsync(GetKeyProcessRunning(), hash));
            batchTasks.Add(batch.StringSetAsync(GetKeyProcessStatus(processId),(int)status.Id));
            batchTasks.Add(batch.HashSetAsync(GetKeyProcessStatusSetTime(), hash, _runtime.RuntimeDateTimeNow.Ticks));

            if (await db.HashExistsAsync(GetKeyForWorkflowProcessRuntimes(), hash).ConfigureAwait(false))
            {
                RedisValue runtimeId = await db.HashGetAsync(GetKeyForWorkflowProcessRuntimes(), hash).ConfigureAwait(false);

                if (runtimeId != _runtime.Id)
                {
                    batchTasks.Add(batch.SetRemoveAsync(GetKeyForWorkflowRuntimeProcesses(runtimeId), hash));
                }
            }

            batchTasks.Add(batch.HashSetAsync(GetKeyForWorkflowProcessRuntimes(), hash, _runtime.Id));

            if (!exists)
            {
                batchTasks.Add(batch.SetAddAsync(GetKeyForWorkflowRuntimeProcesses(_runtime.Id), hash));
            }
            
            batch.Execute();
#pragma warning restore 4014
            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }

        private async Task<List<Task>> AddDeleteTimersOperationsToBatchAsync(Guid processId, List<string> timersIgnoreList, IDatabase db, IBatch batch)
        {
            var batchTasks = new List<Task>();
            
            RedisValue[] timerNames = await db.SetMembersAsync(GetKeyProcessTimers(processId)).ConfigureAwait(false);

            var redisTimers = await Task.WhenAll(timerNames.Select(async x => new {Name = x, Value = await db.StringGetAsync(GetKeyProcessTimer(processId, x)).ConfigureAwait(false)})).ConfigureAwait(false);
            var timers =
                    redisTimers
                    .Where(he => !timersIgnoreList.Contains(he.Name))
                    .Select(he => new TimerToExecute {Name = he.Name, ProcessId = processId, TimerId = Guid.Parse(he.Value)})
                    .ToList();

            foreach (TimerToExecute timer in timers)
            {
#pragma warning disable 4014
                batchTasks.Add(batch.SortedSetRemoveAsync(GetKeyTimerTime(), timer.TimerId.ToString("N")));
                batchTasks.Add(batch.KeyDeleteAsync(GetKeyProcessTimer(processId, timer.Name)));
                batchTasks.Add(batch.SetRemoveAsync(GetKeyProcessTimers(processId), timer.Name));
                batchTasks.Add(batch.HashDeleteAsync(GetKeyTimerIgnore(), timer.TimerId.ToString("N")));
                batchTasks.Add(batch.KeyDeleteAsync(GetKeyTimerIgnoreLock(timer.TimerId)));
                batchTasks.Add(batch.HashDeleteAsync(GetKeyTimer(), timer.TimerId.ToString("N")));
#pragma warning restore 4014
            }

            return batchTasks;
        }

        private async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(IDatabase db, string runtimeId)
        {
            RedisValue statusValue = await db.HashGetAsync(GetKeyForWorkflowRuntimeStatuses(), runtimeId).ConfigureAwait(false);
            if (statusValue.HasValue)
            {
                var status = (RuntimeStatus)Enum.Parse(typeof(RuntimeStatus), statusValue);

                RedisValue lockValue = await db.StringGetAsync(GetKeyForWorkflowRuntimeLocks(runtimeId)).ConfigureAwait(false);

                DateTime? lastAliveSignal = default;

                double? lastAliveSignalValue = await db.SortedSetScoreAsync(GetKeyForWorkflowLastAliveSignals(), runtimeId).ConfigureAwait(false);

                if (lastAliveSignalValue.HasValue)
                {
                    lastAliveSignal = _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(lastAliveSignalValue.Value));
                }

                RedisValue restorerIdValue = await db.HashGetAsync(GetKeyForWorkflowRuntimeRestorers(), runtimeId).ConfigureAwait(false);

                string sortedSetKey = GetKeyForWorkflowRuntimeTimer(TimerCategory.Timer.ToString());

                DateTime? nextTimerTime = default;

                double? nextTimerTimeValue = await db.SortedSetScoreAsync(sortedSetKey, runtimeId).ConfigureAwait(false);

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
       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeByProcessIdAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            RedisValue processInstanceValue = await db.StringGetAsync(GetKeyForProcessInstance(processId)).ConfigureAwait(false);

            if (!processInstanceValue.HasValue)
            {
                throw new ProcessNotFoundException(processId);
            }

            WorkflowProcessInstance processInstance = JsonConvert.DeserializeObject<WorkflowProcessInstance>(processInstanceValue);

            if (!processInstance.SchemeId.HasValue)
            {
                throw SchemeNotFoundException.Create(processId, SchemeLocation.WorkflowProcessInstance);
            }

            SchemeDefinition<XElement> schemeDefinition = await GetProcessSchemeBySchemeIdAsync(processInstance.SchemeId.Value).ConfigureAwait(false);
            schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
            return schemeDefinition;
        }

        /// <summary>
        /// Gets not parsed scheme by id
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeBySchemeIdAsync(Guid schemeId)
        {
            IDatabase db = _connector.GetDatabase();
            RedisValue processSchemeValue = await db.StringGetAsync(GetKeyForProcessScheme(schemeId)).ConfigureAwait(false);

            if (!processSchemeValue.HasValue)
            {
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
            }

            WorkflowProcessScheme processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(processSchemeValue);

            if (String.IsNullOrEmpty(processScheme.Scheme))
            {
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
            }

            string key = GetKeyForCurrentScheme(processScheme.RootSchemeId.HasValue ? processScheme.RootSchemeCode : processScheme.SchemeCode,
                HashHelper.GenerateStringHash(processScheme.DefiningParameters));

            bool isObsolete = !await db.KeyExistsAsync(key).ConfigureAwait(false);

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
       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeWithParametersAsync(string schemeCode, string parameters, Guid? rootSchemeId, bool ignoreObsolete)
        {
            IDatabase db = _connector.GetDatabase();

            if (rootSchemeId.HasValue)
            {
                RedisValue schemeIdValue = await db.HashGetAsync(GetKeySchemeHierarchy(rootSchemeId.Value), schemeCode).ConfigureAwait(false);
                if (!schemeIdValue.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
                }

                var schemeId = Guid.Parse(schemeIdValue);

                RedisValue processSchemeValue = await db.StringGetAsync(GetKeyForProcessScheme(schemeId)).ConfigureAwait(false);

                if (!processSchemeValue.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
                }

                WorkflowProcessScheme processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(processSchemeValue);
                bool isObsolete = !await db.KeyExistsAsync(GetKeyForCurrentScheme(processScheme.RootSchemeCode, HashHelper.GenerateStringHash(parameters))).ConfigureAwait(false);

                if (!ignoreObsolete && isObsolete)
                {
                    throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
                }

                return ConvertToSchemeDefinition(schemeId, isObsolete, processScheme);
            }
            else
            {
                RedisValue schemeIdValue = await db.StringGetAsync(GetKeyForCurrentScheme(schemeCode, HashHelper.GenerateStringHash(parameters))).ConfigureAwait(false);

                if (!schemeIdValue.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
                }

                var schemeId = Guid.Parse(schemeIdValue);

                RedisValue processSchemeValue = await db.StringGetAsync(GetKeyForProcessScheme(schemeId)).ConfigureAwait(false);

                if (!processSchemeValue.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
                }

                WorkflowProcessScheme processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(processSchemeValue);

                return ConvertToSchemeDefinition(schemeId, false, processScheme);
            }
        }

        /// <summary>
        /// Gets not parsed scheme by scheme name  
        /// </summary>
        /// <param name="code">Name of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
       public virtual async Task<XElement> GetSchemeAsync(string code)
        {
            RedisValue scheme = await _connector.GetDatabase().StringGetAsync(GetKeyForScheme(code)).ConfigureAwait(false);

            if (!scheme.HasValue || String.IsNullOrEmpty(scheme)) //-V3027
            {
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);
            }

            return XElement.Parse(scheme);
        }

        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="scheme">Not parsed scheme of the process</param>
        /// <exception cref="SchemeAlreadyExistsException"></exception>
       public virtual async Task<SchemeDefinition<XElement>> SaveSchemeAsync(SchemeDefinition<XElement> scheme)
        {
            IDatabase db = _connector.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            if (!scheme.IsObsolete) //there is only one current scheme can exists 
            {
                if (!scheme.RootSchemeId.HasValue)
                {
                    string key = GetKeyForCurrentScheme(scheme.SchemeCode, HashHelper.GenerateStringHash(scheme.DefiningParameters));
                    tran.AddCondition(Condition.KeyNotExists(key));
#pragma warning disable 4014
                    tran.StringSetAsync(key, $"{scheme.Id:N}");
                    tran.SetAddAsync(GetKeyForCurrentSchemes(scheme.SchemeCode), key);
#pragma warning restore 4014
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

            string newSchemeValue = JsonConvert.SerializeObject(newProcessScheme);
            string newSchemeKey = GetKeyForProcessScheme(scheme.Id);

            tran.AddCondition(Condition.KeyNotExists(newSchemeKey));

#pragma warning disable 4014
            tran.StringSetAsync(newSchemeKey, newSchemeValue);

            if (scheme.RootSchemeId.HasValue)
            {
                tran.HashSetAsync(GetKeySchemeHierarchy(scheme.RootSchemeId.Value), scheme.SchemeCode, scheme.Id.ToString("N"));
            }
#pragma warning restore 4014

            bool result = await tran.ExecuteAsync().ConfigureAwait(false);

            if (!result)
            {
                throw SchemeAlreadyExistsException.Create(scheme.SchemeCode, SchemeLocation.WorkflowProcessScheme, scheme.DefiningParameters);
            }

            return null;
        }

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">Parameters for creating the scheme</param>
       public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode, IDictionary<string, object> parameters)
        {
            string definingParameters = DefiningParametersSerializer.Serialize(parameters);
            IDatabase db = _connector.GetDatabase();
            string key = GetKeyForCurrentScheme(schemeCode, HashHelper.GenerateStringHash(definingParameters));

            var batchTasks = new List<Task>();
            
            IBatch batch = db.CreateBatch();
            batchTasks.Add(batch.KeyDeleteAsync(key));
            batchTasks.Add(batch.SetRemoveAsync(GetKeyForCurrentSchemes(schemeCode), key));

            batch.Execute();

            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
       public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode)
        {
            IDatabase db = _connector.GetDatabase();

            RedisValue[] keys = await db.SetMembersAsync(GetKeyForCurrentSchemes(schemeCode)).ConfigureAwait(false);

            var batchTasks = new List<Task>();
            
            IBatch batch = db.CreateBatch();

            foreach(RedisValue k in keys.Where(k => k.HasValue))
            {
                batchTasks.Add(batch.KeyDeleteAsync(k.ToString()));
            }
            batchTasks.Add(batch.KeyDeleteAsync(GetKeyForCurrentSchemes(schemeCode)));
            
            batch.Execute();

            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }


        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="inlinedSchemes">Scheme codes to be inlined into this scheme</param>
        /// <param name="scheme">Not parsed scheme</param>
        /// <param name="canBeInlined">if true - this scheme can be inlined into another schemes</param>
        /// <param name="tags">Scheme tags</param>
       public virtual async Task SaveSchemeAsync(string schemeCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags)
        {
            IDatabase db = _connector.GetDatabase();
            ITransaction tran = db.CreateTransaction();
#pragma warning disable 4014
            tran.StringSetAsync(GetKeyForScheme(schemeCode), scheme);
#pragma warning restore 4014

            string keyForInlined = GetKeyForInlined();
            string keyCanBeInlined = GetKeyCanBeInlined();
            
#pragma warning disable 4014
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
#pragma warning restore 4014

            await tran.ExecuteAsync().ConfigureAwait(false);
        }

       public virtual async Task<List<string>> GetInlinedSchemeCodesAsync()
        {
            RedisValue[] keys = await _connector.GetDatabase().HashKeysAsync(GetKeyCanBeInlined()).ConfigureAwait(false);
            return keys.Select(c => c.ToString()).ToList();
        }

       public virtual async Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode)
        {
            IDatabase db = _connector.GetDatabase();

            HashEntry[] pairs = await db.HashGetAllAsync(GetKeyForInlined()).ConfigureAwait(false);

            var res = new List<string>();

            foreach (HashEntry pair in pairs)
            {
                List<string> inlined = JsonConvert.DeserializeObject<List<string>>(pair.Value.ToString());
                if (inlined.Contains(schemeCode))
                {
                    res.Add(pair.Name.ToString());
                }
            }
            return res;
        }

       public virtual async Task<List<string>> SearchSchemesByTagsAsync(params string[] tags)
        {
            return await SearchSchemesByTagsAsync(tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags)
        {
            var tagsList = tags?.ToList();
            bool isEmpty = tagsList == null || !tagsList.Any();

            IDatabase db = _connector.GetDatabase();

            HashEntry[] pairs = await db.HashGetAllAsync(GetKeyForTags()).ConfigureAwait(false);

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

                    if (storedTags.Any(st => tagsList.Contains(st)))
                    {
                        res.Add(pair.Name.ToString());
                    }
                }
            }

            return res;
        }

       public virtual async Task AddSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await AddSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await UpdateTagsAsync(schemeCode, (schemeTags) => tags.Concat(schemeTags).ToList()).ConfigureAwait(false);
        }

       public virtual async Task RemoveSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await RemoveSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await UpdateTagsAsync(schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList()).ConfigureAwait(false);
        }

       public virtual async Task SetSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await SetSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await UpdateTagsAsync(schemeCode, (schemeTags) => tags.ToList()).ConfigureAwait(false);
        }

        private async Task UpdateTagsAsync(string schemeCode, Func<List<string>, List<string>> getNewTags)
        {
            IDatabase db = _connector.GetDatabase();
            string key = GetKeyForTags();
            ITransaction tran = db.CreateTransaction();

            RedisValue dbValue = await db.HashGetAsync(key, schemeCode).ConfigureAwait(false);

            string dbTags = "";

            if (dbValue.HasValue)
            {
                dbTags = dbValue.ToString();
            }

            List<string> newTags = getNewTags(TagHelper.FromTagStringForDatabase(dbTags));

#pragma warning disable 4014
            tran.HashSetAsync(key, schemeCode, TagHelper.ToTagStringForDatabase(newTags));
#pragma warning restore 4014

            string scheme = await db.StringGetAsync(GetKeyForScheme(schemeCode)).ConfigureAwait(false);
            scheme = _runtime.Builder.ReplaceTagsInScheme(scheme, newTags);
#pragma warning disable 4014
            tran.StringSetAsync(GetKeyForScheme(schemeCode), scheme);
#pragma warning restore 4014

            bool res = await tran.ExecuteAsync().ConfigureAwait(false);

            if (!res)
            {
                throw new ImpossibleToUpdateSchemeTagsException(schemeCode, "Transaction failed (may be because of concurrency)");
            }
        }

        #endregion

        #region Implementation of IPersistenceProvider

       public virtual async Task DeleteInactiveTimersByProcessIdAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            string ignoreKey = GetKeyTimerIgnore();
            IBatch batch = db.CreateBatch();

            RedisValue[] timerNames = await db.SetMembersAsync(GetKeyProcessTimers(processId)).ConfigureAwait(false);

            var redisTimers = await Task.WhenAll(timerNames.Select(async x => new {Name = x, Value = await db.StringGetAsync(GetKeyProcessTimer(processId, x)).ConfigureAwait(false)}))
                .ConfigureAwait(false);

            var timers = redisTimers
                .Where(he => db.HashExists(ignoreKey, he.Value))
                .Select(he => new TimerToExecute {Name = he.Name, ProcessId = processId, TimerId = Guid.Parse(he.Value)})
                .ToList();
            
            var batchTasks = new List<Task>();

            foreach (TimerToExecute timer in timers)
            {
                batchTasks.Add(batch.SortedSetRemoveAsync(GetKeyTimerTime(), timer.TimerId.ToString("N")));
                batchTasks.Add(batch.KeyDeleteAsync(GetKeyProcessTimer(processId, timer.Name)));
                batchTasks.Add(batch.SetRemoveAsync(GetKeyProcessTimers(processId), timer.Name));
                batchTasks.Add(batch.HashDeleteAsync(ignoreKey, timer.TimerId.ToString("N")));
                batchTasks.Add(batch.HashDeleteAsync(GetKeyTimer(), timer.TimerId.ToString("N")));
            }

            batch.Execute();

            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }

       public virtual async Task DeleteTimerAsync(Guid timerId)
        {
            IDatabase db = _connector.GetDatabase();

            string timerKey = timerId.ToString("N");

            RedisValue timerValue = await db.HashGetAsync(GetKeyTimer(), timerKey).ConfigureAwait(false);

            if (timerValue.HasValue)
            {
                TimerToExecute timer = JsonConvert.DeserializeObject<TimerToExecute>(timerValue);

                var batchTasks = new List<Task>();
                
                IBatch batch = db.CreateBatch();

                batchTasks.Add(batch.SortedSetRemoveAsync(GetKeyTimerTime(), timerKey));
                batchTasks.Add(batch.KeyDeleteAsync(GetKeyProcessTimer(timer.ProcessId, timer.Name)));
                batchTasks.Add(batch.SetRemoveAsync(GetKeyProcessTimers(timer.ProcessId), timer.Name));
                batchTasks.Add(batch.HashDeleteAsync(GetKeyTimerIgnore(), timerKey));
                batchTasks.Add(batch.KeyDeleteAsync(GetKeyTimerIgnoreLock(timerKey)));
                batchTasks.Add(batch.HashDeleteAsync(GetKeyTimer(), timerKey));

                batch.Execute();

                await Task.WhenAll(batchTasks).ConfigureAwait(false);
            }
        }

       public virtual async Task<List<Guid>> GetRunningProcessesAsync(string runtimeId = null)
        {
            IDatabase db = _connector.GetDatabase();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                return (await db.SetMembersAsync(GetKeyForWorkflowRuntimeProcesses(runtimeId)).ConfigureAwait(false)).Where(rp => rp.HasValue)
                    .Select(x => Guid.Parse(x)).ToList();
            }

            return (await db.HashKeysAsync(GetKeyProcessRunning()).ConfigureAwait(false)).Where(rp => rp.HasValue).Select(x => Guid.Parse(x)).ToList();
        }

       public virtual async Task <WorkflowRuntimeModel> CreateWorkflowRuntimeAsync(string runtimeId, RuntimeStatus status)
        {
            IDatabase db = _connector.GetDatabase();

            IBatch batch = db.CreateBatch();

            var runtime = new WorkflowRuntimeModel() {RuntimeId = runtimeId, Lock = Guid.NewGuid(), Status = status};

            var batchTasks = new List<Task>
            {
                batch.HashSetAsync(GetKeyForWorkflowRuntimeStatuses(), runtimeId, status.ToString()),
                batch.StringSetAsync(GetKeyForWorkflowRuntimeLocks(runtimeId), runtime.Lock.ToString("N")),
                batch.SetAddAsync(GetKeyForWorkflowRuntimeStatusSet(status), runtimeId)
            };

            batch.Execute();

            await Task.WhenAll(batchTasks).ConfigureAwait(false);

            return runtime;
        }

       public virtual async Task DeleteWorkflowRuntimeAsync(string name)
        {
            IDatabase db = _connector.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            RedisValue statusValue = await db.HashGetAsync(GetKeyForWorkflowRuntimeStatuses(), name).ConfigureAwait(false);

            if (statusValue.HasValue)
            {
                var status = (RuntimeStatus)Enum.Parse(typeof(RuntimeStatus), statusValue);

#pragma warning disable 4014
                tran.HashDeleteAsync(GetKeyForWorkflowRuntimeStatuses(), name);
                tran.KeyDeleteAsync(GetKeyForWorkflowRuntimeLocks(name));
                tran.SetRemoveAsync(GetKeyForWorkflowRuntimeStatusSet(status), name);
                tran.SortedSetRemoveAsync(GetKeyForWorkflowLastAliveSignals(), name);
                tran.HashDeleteAsync(GetKeyForWorkflowRuntimeRestorers(), name);

                tran.SortedSetRemoveAsync(GetKeyForWorkflowRuntimeTimer(TimerCategory.Timer.ToString()), name);
                tran.SortedSetRemoveAsync(GetKeyForWorkflowRuntimeTimer(TimerCategory.ServiceTimer.ToString()), name);

                tran.KeyDeleteAsync(GetKeyForWorkflowRuntimeProcesses(name));
#pragma warning restore 4014
            }

            await tran.ExecuteAsync().ConfigureAwait(false);
        }

       public async Task<List<ProcessInstanceItem>> GetProcessInstancesAsync(List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
       {
            // IDatabase db = _connector.GetDatabase();
            //
            // var processIdsLists = (await db.ListRangeAsync(GetKeyForSubprocesses(rootProcessId)).
            //     ConfigureAwait(false)).
            //     Where(v => v.HasValue).Select(v => new Guid(v.ToString())).ToList();
            //
            //
            //
            // var documentCollectionKey = GetKeyForSortedDocuments();
            //
            // var count = (int)db.SortedSetLength(documentCollectionKey);
            //
            // var keys = db.SortedSetRangeByRank(documentCollectionKey, page * pageSize, (page + 1) * pageSize - 1, Order.Descending);
            //
            // var docs = db.StringGet(keys.Select(k => (RedisKey)GetKeyForDocument(new Guid((string)k))).ToArray());
            //
            //
            // RedisValue[] redisProcesses = await Task.WhenAll(processIdsLists.Select(processId => db.StringGetAsync(GetKeyForProcessInstance(processId)))).ConfigureAwait(false);
            //
            // List<WorkflowProcessInstance> instances =
            //     redisProcesses
            //         .Where(v => v.HasValue)
            //         .Select(v =>
            //         {
            //             WorkflowProcessInstance processInstance = JsonConvert.DeserializeObject<WorkflowProcessInstance>(v);
            //
            //             if (!processInstance.SchemeId.HasValue)
            //             {
            //                 throw SchemeNotFoundException.Create(processInstance.Id, SchemeLocation.WorkflowProcessInstance);
            //             }
            //
            //             return processInstance;
            //         }).ToList();
            //
            //
            // var startingTransitions = (await Task.WhenAll(instances.Select(i => i.SchemeId.Value).Distinct().Select(async schemeId =>
            // {
            //     RedisValue s = await db.StringGetAsync(GetKeyForProcessScheme(schemeId)).ConfigureAwait(false);
            //     if (!s.HasValue)
            //     {
            //         throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
            //     }
            //
            //     WorkflowProcessScheme processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(s);
            //
            //     return (schemeId, startingTransition: processScheme.StartingTransition);
            // })).ConfigureAwait(false)).ToDictionary(t => t.schemeId, t => t.startingTransition);
            //

            // return ProcessInstanceTreeItem.Create(rootProcessId, processInfo, startingTransitions);
            
            throw new NotSupportedException("Select all doesn't support in Redis");
       }

       public Task<int> GetProcessInstancesCountAsync()
       {
           throw new NotSupportedException("Getting count of items doesn't  support in Redis");
       }

       public Task<List<SchemeItem>> GetSchemesAsync(List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
       {
           throw new NotSupportedException("Getting count of items doesn't  support in Redis");
       }
       
       public Task<int> GetSchemesCountAsync()
       {
           throw new NotSupportedException("Getting count of items doesn't  support in Redis");
       }

       public virtual async Task<WorkflowRuntimeModel> UpdateWorkflowRuntimeStatusAsync(WorkflowRuntimeModel runtime, RuntimeStatus status)
        {
            IDatabase db = _connector.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            Guid oldLock = runtime.Lock;
            runtime.Lock = Guid.NewGuid();
            runtime.Status = status;

            tran.AddCondition(Condition.StringEqual(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), oldLock.ToString("N")));

            RedisValue statusValue = await db.HashGetAsync(GetKeyForWorkflowRuntimeStatuses(), runtime.RuntimeId).ConfigureAwait(false);
            if (statusValue.HasValue)
            {
                var currentStatus = (RuntimeStatus)Enum.Parse(typeof(RuntimeStatus), statusValue);
#pragma warning disable 4014
                tran.SetRemoveAsync(GetKeyForWorkflowRuntimeStatusSet(currentStatus), runtime.RuntimeId);
#pragma warning restore 4014
            }

#pragma warning disable 4014
            tran.HashSetAsync(GetKeyForWorkflowRuntimeStatuses(), runtime.RuntimeId, status.ToString());
            tran.StringSetAsync(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), runtime.Lock.ToString("N"));
            tran.SetAddAsync(GetKeyForWorkflowRuntimeStatusSet(status), runtime.RuntimeId);
#pragma warning restore 4014

            bool res = await tran.ExecuteAsync().ConfigureAwait(false);

            if (!res)
            {
                throw new ImpossibleToSetRuntimeStatusException();
            }

            return runtime;
        }

       public virtual async  Task<(bool Success, WorkflowRuntimeModel UpdatedModel)> UpdateWorkflowRuntimeRestorerAsync(WorkflowRuntimeModel runtime, string restorerId)
        {
            IDatabase db = _connector.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            Guid oldLock = runtime.Lock;
            runtime.Lock = Guid.NewGuid();
            runtime.RestorerId = restorerId;

            tran.AddCondition(Condition.StringEqual(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), oldLock.ToString("N")));

#pragma warning disable 4014
            tran.HashSetAsync(GetKeyForWorkflowRuntimeRestorers(), runtime.RuntimeId, runtime.RestorerId);
            tran.StringSetAsync(GetKeyForWorkflowRuntimeLocks(runtime.RuntimeId), runtime.Lock.ToString("N"));
#pragma warning restore 4014
            
            bool res = await tran.ExecuteAsync().ConfigureAwait(false);

            if (!res)
            {
                return (false, null);
            }

            return (true, runtime);
        }

       public virtual async Task<WorkflowRuntimeModel> GetWorkflowRuntimeModelAsync(string runtimeId)
        {
            IDatabase db = _connector.GetDatabase();
            return await GetWorkflowRuntimeStatusAsync(db, runtimeId).ConfigureAwait(false);
        }

       public virtual async Task<bool> MultiServerRuntimesExistAsync()
        {
            List<WorkflowRuntimeModel> runtimes = await GetWorkflowRuntimesAsync().ConfigureAwait(false);

            return runtimes.Any(r =>
                r.RuntimeId != Guid.Empty.ToString() && r.Status != RuntimeStatus.Dead && r.Status != RuntimeStatus.Terminated);
        }

       public virtual async Task<int> SendRuntimeLastAliveSignalAsync()
        {
            IDatabase db = _connector.GetDatabase();
            string runtimeId = _runtime.Id;

            if (!await db.SetContainsAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive), runtimeId).ConfigureAwait(false) &&
                !await db.SetContainsAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.SelfRestore), runtimeId).ConfigureAwait(false))
            {
                return 0;
            }

            double unixTime = (_runtime.RuntimeDateTimeNow.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

            await db.SortedSetAddAsync(GetKeyForWorkflowLastAliveSignals(), runtimeId, unixTime).ConfigureAwait(false);

            return 1;
        }

       public virtual async Task<int> ActiveMultiServerRuntimesCountAsync(string currentRuntimeId)
        {
            IDatabase db = _connector.GetDatabase();

            int result = (int)(
                await db.SetLengthAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive)).ConfigureAwait(false) +
                await db.SetLengthAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Restore)).ConfigureAwait(false) +
                await db.SetLengthAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.SelfRestore)).ConfigureAwait(false)
            );

            if (await db.SetContainsAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive), currentRuntimeId).ConfigureAwait(false))
            {
                result -= 1;
            }
            else if (await db.SetContainsAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Restore), currentRuntimeId).ConfigureAwait(false))
            {
                result -= 1;
            }
            else if (await db.SetContainsAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.SelfRestore), currentRuntimeId).ConfigureAwait(false))
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
                db.StringSet(GetKeyForWorkflowSyncLocks(TimerCategory.Timer), Guid.NewGuid().ToString("N"));
            }

            if (!db.KeyExists(GetKeyForWorkflowSyncLocks(TimerCategory.ServiceTimer)))
            {
                db.StringSet(GetKeyForWorkflowSyncLocks(TimerCategory.ServiceTimer), Guid.NewGuid().ToString("N"));
            }

        }

        /// <summary>
        /// Initialize a process instance in persistence store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ProcessAlreadyExistsException"></exception>
       public virtual async Task InitializeProcessAsync(ProcessInstance processInstance)
        {
            var newProcess = new WorkflowProcessInstance
            {
                Id = processInstance.ProcessId,
                SchemeId = processInstance.SchemeId,
                ActivityName = processInstance.ProcessScheme.InitialActivity.Name,
                StateName = processInstance.ProcessScheme.InitialActivity.State,
                RootProcessId = processInstance.RootProcessId,
                ParentProcessId = processInstance.ParentProcessId,
                TenantId = processInstance.TenantId,
                SubprocessName = processInstance.SubprocessName,
                CreationDate = processInstance.CreationDate
            };

            string processKey = GetKeyForProcessInstance(processInstance.ProcessId);

            IDatabase db = _connector.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyNotExists(processKey));
#pragma warning disable 4014
            tran.StringSetAsync(processKey, JsonConvert.SerializeObject(newProcess));
            if (processInstance.IsSubprocess)
            {
                tran.StringSetAsync(GetKeyForRootProcess(processInstance.ProcessId), processInstance.RootProcessId.ToString("N"));
                tran.ListLeftPushAsync(GetKeyForSubprocesses(processInstance.RootProcessId), processInstance.ProcessId.ToString("N"));
            }
#pragma warning restore 4014

            bool res = await tran.ExecuteAsync().ConfigureAwait(false);

            if (!res)
            {
                throw new ProcessAlreadyExistsException(processInstance.ProcessId);
            }
        }

        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/>  and persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
       public virtual async Task FillProcessParametersAsync(ProcessInstance processInstance)
        {
            await FillPersistedProcessParametersAsync(processInstance).ConfigureAwait(false);
            await FillSystemProcessParametersAsync(processInstance).ConfigureAwait(false);
        }

        /// <summary>
        /// Fills persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
       public virtual async Task FillPersistedProcessParametersAsync(ProcessInstance processInstance)
        {
            var persistenceParameters = processInstance.ProcessScheme.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count);

            IDatabase db = _connector.GetDatabase();
            RedisValue persistedParametersValue = await db.StringGetAsync(GetKeyForProcessPersistence(processInstance.ProcessId)).ConfigureAwait(false);
           
            if (!persistedParametersValue.HasValue)
            {
                return;
            }

            Dictionary<string, string> persistedParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(persistedParametersValue);

            foreach (KeyValuePair<string, string> persistedParameter in persistedParameters)
            {
                parameters.Add(WorkflowProcessInstancePersistenceToParameterDefinitionWithValue(persistenceParameters, persistedParameter));
            }

            processInstance.AddParameters(parameters);
        }
       public virtual async Task FillPersistedProcessParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            var persistenceParameters = processInstance.ProcessScheme.PersistenceParameters.ToList();

            IDatabase db = _connector.GetDatabase();
            RedisValue persistedParametersValue = await db.StringGetAsync(GetKeyForProcessPersistence(processInstance.ProcessId)).ConfigureAwait(false);
            if (!persistedParametersValue.HasValue)
            {
                return;
            }

            KeyValuePair<string, string> persistedParameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(persistedParametersValue).SingleOrDefault(x => x.Key == parameterName);
            processInstance.AddParameter(WorkflowProcessInstancePersistenceToParameterDefinitionWithValue(persistenceParameters, persistedParameter));
        }

        private ParameterDefinitionWithValue WorkflowProcessInstancePersistenceToParameterDefinitionWithValue(List<ParameterDefinition> persistenceParameters, KeyValuePair<string, string> persistedParameter)
        {
            ParameterDefinition parameterDefinition = persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.Key);
            if (parameterDefinition == null)
            {
                parameterDefinition = ParameterDefinition.Create(persistedParameter.Key, typeof(UnknownParameterType), ParameterPurpose.Persistence);
                return ParameterDefinition.Create(parameterDefinition, persistedParameter.Value);
            }
            else
            {
                return ParameterDefinition.Create(parameterDefinition, ParametersSerializer.Deserialize(persistedParameter.Value, parameterDefinition.Type));
            }
        }

        /// <summary>
        /// Fills system <see cref="ParameterPurpose.System"/> parameters of the process
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
       public virtual async Task FillSystemProcessParametersAsync(ProcessInstance processInstance)
        {
            IDatabase db = _connector.GetDatabase();
            RedisValue workflowProcessInstanceValue = await db.StringGetAsync(GetKeyForProcessInstance(processInstance.ProcessId)).ConfigureAwait(false);

            if (!workflowProcessInstanceValue.HasValue)
            {
                throw new ProcessNotFoundException(processInstance.ProcessId);
            }

            WorkflowProcessInstance workflowProcessInstance = JsonConvert.DeserializeObject<WorkflowProcessInstance>(workflowProcessInstanceValue);

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
                    workflowProcessInstance.TenantId),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterSubprocessName.Name),
                    workflowProcessInstance.SubprocessName),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCreationDate.Name),
                    workflowProcessInstance.CreationDate),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterLastTransitionDate.Name),
                    workflowProcessInstance.LastTransitionDate)
            };

            processInstance.AddParameters(parameters);
        }

        /// <summary>
        /// Saves persisted <see cref="ParameterPurpose.Persistence"/> parameters of the process to store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
       public virtual async Task SavePersistenceParametersAsync(ProcessInstance processInstance)
        {
            var parametersToSave = processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Value != null)
                .ToDictionary(ptp => ptp.Name, ptp => GetSerializedValue(ptp));

            var parametersToRemove =
                processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Value == null).Select(ptp => ptp.Name).ToList();

            if (!parametersToSave.Any() && !parametersToRemove.Any())
            {
                return;
            }

            IDatabase db = _connector.GetDatabase();

            string key = GetKeyForProcessPersistence(processInstance.ProcessId);

            RedisValue oldParametersValue = await db.StringGetAsync(key).ConfigureAwait(false);

            if (!oldParametersValue.HasValue)
            {
                if (parametersToSave.Any())
                {
                    await db.StringSetAsync(key, JsonConvert.SerializeObject(parametersToSave)).ConfigureAwait(false);
                }
            }
            else
            {
                Dictionary<string, string> existingParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(oldParametersValue);

                parametersToRemove.ForEach(p => existingParameters.Remove(p));

                foreach (string ptsKey in parametersToSave.Keys)
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
                {
                    await db.StringSetAsync(key, JsonConvert.SerializeObject(existingParameters)).ConfigureAwait(false);
                }
                else
                {
                    await db.KeyDeleteAsync(key).ConfigureAwait(false);
                }
            }
        }
       public virtual async Task SavePersistenceParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            ParameterDefinitionWithValue parameterDefinitionWithValue = processInstance.ProcessParameters.SingleOrDefault(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Name == parameterName);

            var parameter = parameterDefinitionWithValue != null ? new {Key = parameterDefinitionWithValue.Name, Value = GetSerializedValue(parameterDefinitionWithValue)} : null;

            IDatabase db = _connector.GetDatabase();

            string key = GetKeyForProcessPersistence(processInstance.ProcessId);

            RedisValue oldParametersValue = await db.StringGetAsync(key).ConfigureAwait(false);

            if (!oldParametersValue.HasValue && parameter != null)
            {
                await db.StringSetAsync(key, JsonConvert.SerializeObject(new Dictionary<string, dynamic>() { { parameter.Key, parameter.Value } })).ConfigureAwait(false);
            }
            else if (oldParametersValue.HasValue)
            {
                Dictionary<string, string> existingParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(oldParametersValue);

                if (parameter == null)
                {
                    if (existingParameters.ContainsKey(parameterName))
                    {
                        existingParameters.Remove(parameterName);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    string ptsKey = parameter.Key;
                    if (existingParameters.ContainsKey(ptsKey))
                    {
                        existingParameters[ptsKey] = parameter.Value;
                    }
                    else
                    {
                        existingParameters.Add(ptsKey, parameter.Value);
                    }
                }

                if (existingParameters.Any())
                {
                    await db.StringSetAsync(key, JsonConvert.SerializeObject(existingParameters)).ConfigureAwait(false);
                }
                else
                {
                    await db.KeyDeleteAsync(key).ConfigureAwait(false);
                }
            }
        }

        private string GetSerializedValue(ParameterDefinitionWithValue ptp)
        {
            string serializedValue = ptp.Type == typeof(UnknownParameterType) ? (string)ptp.Value : ParametersSerializer.Serialize(ptp.Value, ptp.Type);
            return serializedValue;
        }
        
       public virtual async Task RemoveParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            IDatabase db = _connector.GetDatabase();
            string key = GetKeyForProcessPersistence(processInstance.ProcessId);
            RedisValue persistedParameters = await db.StringGetAsync(key).ConfigureAwait(false);
           
            if (!persistedParameters.HasValue)
            {
                return;
            }

            Dictionary<string, string> existingParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(persistedParameters);

            if(!existingParameters.Keys.Contains(parameterName))
            {
                return;
            }

            existingParameters.Remove(parameterName);

            
            await db.StringSetAsync(key, JsonConvert.SerializeObject(existingParameters)).ConfigureAwait(false);
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

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Initialized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
       public virtual async Task SetWorkflowInitializedAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Initialized, true).ConfigureAwait(false);
        }



        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Idled"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
       public virtual async Task SetWorkflowIdledAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Idled).ConfigureAwait(false);
        }

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Running"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
       public virtual async Task SetWorkflowRunningAsync(ProcessInstance processInstance)
        {
            Guid processId = processInstance.ProcessId;
            await SetRunningStatusAsync(processId).ConfigureAwait(false);
        }

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Finalized"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
       public virtual async Task SetWorkflowFinalizedAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Finalized).ConfigureAwait(false);
        }

        /// <summary>
        /// Set process instance status to <see cref="ProcessStatus.Terminated"/>
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <exception cref="ImpossibleToSetStatusException"></exception>
       public virtual async Task SetWorkflowTerminatedAsync(ProcessInstance processInstance)
        {
            await SetCustomStatusAsync(processInstance.ProcessId, ProcessStatus.Terminated).ConfigureAwait(false);
        }

        public async Task WriteInitialRecordToHistoryAsync(ProcessInstance processInstance)
        {
            if (!_writeToHistory) { return; }

            IDatabase db = _connector.GetDatabase();
            ITransaction tran = db.CreateTransaction();

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

            string key = GetKeyForProcessHistory(_writeSubProcessToRoot && processInstance.IsSubprocess
                ? processInstance.RootProcessId
                : processInstance.ProcessId);
            string json = JsonConvert.SerializeObject(history);

            await db.ListRightPushAsync(key, json).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates system parameters of the process in the store
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <param name="transition">Last executed transition</param>
       public virtual async Task UpdatePersistenceStateAsync(ProcessInstance processInstance, TransitionDefinition transition)
        {
            DateTime startTransitionTime = processInstance.StartTransitionTime ?? _runtime.RuntimeDateTimeNow;
            
            IDatabase db = _connector.GetDatabase();
            string key = GetKeyForProcessInstance(processInstance.ProcessId);
            RedisValue processInstanceValue = await db.StringGetAsync(key).ConfigureAwait(false);

            if (!processInstanceValue.HasValue)
            {
                throw new ProcessNotFoundException(processInstance.ProcessId);
            }

            WorkflowProcessInstance inst = JsonConvert.DeserializeObject<WorkflowProcessInstance>(processInstanceValue);

            ParameterDefinitionWithValue paramIdentityId = await processInstance.GetParameterAsync(DefaultDefinitions.ParameterIdentityId.Name).ConfigureAwait(false);
            ParameterDefinitionWithValue paramImpIdentityId = await processInstance.GetParameterAsync(DefaultDefinitions.ParameterImpersonatedIdentityId.Name).ConfigureAwait(false);
            string identityId = paramIdentityId == null ? String.Empty : (string)paramIdentityId.Value;
            string impIdentityId = paramImpIdentityId == null ? identityId : (string)paramImpIdentityId.Value;

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

            var batchTasks = new List<Task>();
            
            IBatch batch = db.CreateBatch();

            batchTasks.Add(batch.StringSetAsync(key, JsonConvert.SerializeObject(inst)));

            if (_writeToHistory)
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
                    Id = new Guid(),
                    ProcessId = _writeSubProcessToRoot && processInstance.IsSubprocess ? processInstance.RootProcessId : processInstance.ProcessId,
                    TransitionClassifier = transition.Classifier.ToString(),
                    TransitionTime = _runtime.RuntimeDateTimeNow,
                    TriggerName = String.IsNullOrEmpty(processInstance.ExecutedTimer) ? processInstance.CurrentCommand : processInstance.ExecutedTimer,
                    StartTransitionTime = startTransitionTime,
                    TransitionDuration = (int)(_runtime.RuntimeDateTimeNow - startTransitionTime).TotalMilliseconds
                };

                batchTasks.Add(batch.ListRightPushAsync(
                    GetKeyForProcessHistory(_writeSubProcessToRoot && processInstance.IsSubprocess
                        ? processInstance.RootProcessId
                        : processInstance.ProcessId), JsonConvert.SerializeObject(history)));
            }

            batch.Execute();

            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks existence of the process
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
       public virtual async Task<bool> IsProcessExistsAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            return await db.KeyExistsAsync(GetKeyForProcessInstance(processId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns status of the process <see cref="ProcessStatus"/>
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns>Status of the process</returns>
       public virtual async Task<ProcessStatus> GetInstanceStatusAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            RedisValue statusValue = await db.StringGetAsync(GetKeyProcessStatus(processId)).ConfigureAwait(false);
            if (!statusValue.HasValue)
            {
                return ProcessStatus.NotFound;
            }

            int statusId = Int32.Parse(statusValue);
            ProcessStatus status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == statusId);
            return status ?? ProcessStatus.Unknown;
        }

        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process which changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
       public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance)
        {
            await BindProcessToNewSchemeAsync(processInstance, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves information about changed scheme to the store
        /// </summary>
        /// <param name="processInstance">Instance of the process which changed scheme <see cref="ProcessInstance.ProcessScheme"/></param>
        /// <param name="resetIsDeterminingParametersChanged">True if required to reset IsDeterminingParametersChanged flag <see cref="ProcessInstance.IsDeterminingParametersChanged"/></param>
        /// <exception cref="ProcessNotFoundException"></exception>
       public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            IDatabase db = _connector.GetDatabase();
            string key = GetKeyForProcessInstance(processInstance.ProcessId);
            RedisValue processInstanceValue = await db.StringGetAsync(key).ConfigureAwait(false);

            if (!processInstanceValue.HasValue)
            {
                throw new ProcessNotFoundException(processInstance.ProcessId);
            }

            WorkflowProcessInstance inst = JsonConvert.DeserializeObject<WorkflowProcessInstance>(processInstanceValue);

            inst.SchemeId = processInstance.SchemeId;
            if (resetIsDeterminingParametersChanged)
            {
                inst.IsDeterminingParametersChanged = false;
            }

            await db.StringSetAsync(key, JsonConvert.SerializeObject(inst)).ConfigureAwait(false);
        }


       public virtual async Task RegisterTimerAsync(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            IDatabase db = _connector.GetDatabase();
            double unixTime = (nextExecutionDateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;


            var timerId = Guid.NewGuid();
            var timerToExecute = new TimerToExecute() {ProcessId = processId, RootProcessId = rootProcessId, Name = name, TimerId = timerId};

            ITransaction tran = db.CreateTransaction();

            if (notOverrideIfExists)
            {
                tran.AddCondition(Condition.KeyNotExists(GetKeyProcessTimer(processId, name)));
            }

#pragma warning disable 4014
            tran.SortedSetAddAsync(GetKeyTimerTime(), timerId.ToString("N"), unixTime);
            tran.StringSetAsync(GetKeyProcessTimer(processId, name), timerId.ToString("N"));
            tran.SetAddAsync(GetKeyProcessTimers(processId), name);
            tran.HashSetAsync(GetKeyTimer(), timerId.ToString("N"), JsonConvert.SerializeObject(timerToExecute));
#pragma warning restore 4014
            
            await tran.ExecuteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Removes all timers from the store, exclude listed in ignore list
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <param name="timersIgnoreList">Ignore list</param>
       public virtual async Task ClearTimersAsync(Guid processId, List<string> timersIgnoreList)
        {
            IDatabase db = _connector.GetDatabase();
            IBatch batch = db.CreateBatch();

            Task<List<Task>> batchTasks = AddDeleteTimersOperationsToBatchAsync(processId, timersIgnoreList, db, batch);

            batch.Execute();

            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }

       public virtual async Task<int> SetTimerIgnoreAsync(Guid timerId)
        {
            IDatabase db = _connector.GetDatabase();
            string keyTimerIgnore = GetKeyTimerIgnore();
            string timerIdValue = timerId.ToString("N");

            ITransaction tran = db.CreateTransaction();
            ConditionResult conditionResult = tran.AddCondition(Condition.KeyNotExists(GetKeyTimerIgnoreLock(timerIdValue)));
#pragma warning disable 4014
            tran.HashSetAsync(keyTimerIgnore, timerIdValue, true);
            tran.StringSetAsync(GetKeyTimerIgnoreLock(timerIdValue), String.Empty);
#pragma warning restore 4014
            await tran.ExecuteAsync().ConfigureAwait(false);

            return conditionResult.WasSatisfied ? 1 : 0;
        }

       public virtual async Task<List<Core.Model.WorkflowTimer>> GetTopTimersToExecuteAsync(int top)
        {
            double unixTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            IDatabase db = _connector.GetDatabase();

            var timerIds = (await db.SortedSetRangeByScoreWithScoresAsync(GetKeyTimerTime(), Double.NegativeInfinity, unixTime, take: top).ConfigureAwait(false))
                .Where(rv => rv.Element.HasValue).ToList();

            var result = new List<Core.Model.WorkflowTimer>();

            if (!timerIds.Any())
            {
                return result;
            }

            foreach (SortedSetEntry timerIdValue in timerIds)
            {
                RedisValue timerValue = await db.HashGetAsync(GetKeyTimer(), timerIdValue.Element).ConfigureAwait(false);
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
       public virtual async Task DeleteProcessAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            await AddDeleteTimersOperationsToBatchAsync(processId, new List<string>(), db, tran).ConfigureAwait(false);
#pragma warning disable 4014
            tran.KeyDeleteAsync(GetKeyForProcessInstance(processId));
            tran.KeyDeleteAsync(GetKeyForProcessPersistence(processId));
            tran.KeyDeleteAsync(GetKeyForProcessHistory(processId));
            string hash = $"{processId:N}";
            tran.KeyDeleteAsync(GetKeyProcessStatus(hash));
            tran.HashDeleteAsync(GetKeyProcessRunning(), hash);
            tran.HashDeleteAsync(GetKeyProcessStatusSetTime(), hash);
#pragma warning restore 4014
            //Timer deletion
            RedisValue[] timerNames = await db.SetMembersAsync(GetKeyProcessTimers(processId)).ConfigureAwait(false);

            var timerKeys = new List<string>();
            
#pragma warning disable 4014
            foreach(RedisValue tn in timerNames)
            {
                string keyProcessTimer = GetKeyProcessTimer(processId, tn);
                RedisValue timerId = await db.StringGetAsync(keyProcessTimer).ConfigureAwait(false);

                if (timerId.HasValue)
                {
                    timerKeys.Add(timerId);
                }
                
                tran.KeyDeleteAsync(keyProcessTimer);
            }
            tran.KeyDeleteAsync(GetKeyProcessTimers(processId));

            timerKeys = timerKeys.Distinct().ToList();

            foreach (string timerKey in timerKeys)
            {
                tran.SortedSetRemoveAsync(GetKeyTimerTime(), timerKey);
                tran.HashDeleteAsync(GetKeyTimerIgnore(), timerKey);
                tran.KeyDeleteAsync(GetKeyTimerIgnoreLock(timerKey));
                tran.HashDeleteAsync(GetKeyTimer(), timerKey);
            }

#pragma warning restore 4014

            RedisValue runtimeId = await db.HashGetAsync(GetKeyForWorkflowProcessRuntimes(), hash).ConfigureAwait(false);

#pragma warning disable 4014
            tran.SetRemoveAsync(GetKeyForWorkflowRuntimeProcesses(runtimeId), hash);
            tran.HashDeleteAsync(GetKeyForWorkflowProcessRuntimes(), hash);
#pragma warning restore 4014

            RedisValue rootProcessId = await db.StringGetAsync(GetKeyForRootProcess(processId)).ConfigureAwait(false);
          
            if (rootProcessId.HasValue)
            {
#pragma warning disable 4014
                tran.ListRemoveAsync(GetKeyForSubprocesses(new Guid(rootProcessId.ToString())), processId.ToString("N"));
                tran.KeyDeleteAsync(GetKeyForRootProcess(processId));
#pragma warning restore 4014
            }
            
            await DropWorkflowInboxInternalAsync(processId, tran).ConfigureAwait(false);
            await DropApprovalHistoryByProcessIdInternalAsync(processId, tran).ConfigureAwait(false);
          
            await tran.ExecuteAsync().ConfigureAwait(false);
            
        }

        /// <summary>
        /// Remove all information about the process from the store
        /// </summary>
        /// <param name="processIds">List of ids of the process</param>
       public virtual async Task DeleteProcessAsync(Guid[] processIds)
        {
            foreach (Guid processId in processIds)
            {
                await DeleteProcessAsync(processId).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Saves a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
       public virtual async Task SaveGlobalParameterAsync<T>(string type, string name, T value)
        {
            IDatabase db = _connector.GetDatabase();
            await db.HashSetAsync(GetKeyGlobalParameter(type), name, JsonConvert.SerializeObject(value)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Value of the parameter</returns>
       public virtual async Task<T> LoadGlobalParameterAsync<T>(string type, string name)
        {
            IDatabase db = _connector.GetDatabase();
            RedisValue value = await db.HashGetAsync(GetKeyGlobalParameter(type), name).ConfigureAwait(false);
            if (!value.HasValue)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Returns a global parameter value
        /// </summary>
        /// <typeparam name="T">System type of the parameter</typeparam>
        /// <param name="type">Logical type of the parameter</param>
        /// <returns>List of the values of the parameters</returns>
       public virtual async Task<List<T>> LoadGlobalParametersAsync<T>(string type)
        {
            IDatabase db = _connector.GetDatabase();
            return (await db.HashGetAllAsync(GetKeyGlobalParameter(type)).ConfigureAwait(false)).Where(he => he.Value.HasValue).Select(he => JsonConvert.DeserializeObject<T>(he.Value)).ToList();
        }

        /// <summary>
        /// Deletes a global parameter
        /// </summary>
        /// <param name="type">Logical type of the parameter</param>
        /// <param name="name">Name of the parameter</param>
       public virtual async Task DeleteGlobalParametersAsync(string type, string name = null)
        {
            IDatabase db = _connector.GetDatabase();
            if (name != null)
            {
                await db.HashDeleteAsync(GetKeyGlobalParameter(type), name).ConfigureAwait(false);
            }
            else
            {
                await db.KeyDeleteAsync(GetKeyGlobalParameter(type)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
       public virtual async Task<List<ProcessHistoryItem>> GetProcessHistoryAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();

            return (await db.ListRangeAsync(GetKeyForProcessHistory(processId)).ConfigureAwait(false))
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
                    TriggerName = hi.TriggerName,
                    StartTransitionTime = hi.StartTransitionTime,
                    TransitionDuration = hi.TransitionDuration
                })
                .ToList();
        }

       public virtual async Task<List<ProcessTimer>> GetTimersForProcessAsync(Guid processId)
        {
            return await GetTimersForProcessAsync(processId, false).ConfigureAwait(false);
        }

       public virtual async Task<List<ProcessTimer>> GetActiveTimersForProcessAsync(Guid processId)
        {
            return await GetTimersForProcessAsync(processId, true).ConfigureAwait(false);
        }

        private async Task<List<ProcessTimer>> GetTimersForProcessAsync(Guid processId, bool excludeIgnored)
        {
            IDatabase db = _connector.GetDatabase();

            RedisValue[] timerNames = await db.SetMembersAsync(GetKeyProcessTimers(processId)).ConfigureAwait(false);

            var timerIds = (await Task.WhenAll(timerNames.Select(async x => new { Name = x, Id = Guid.Parse(await db.StringGetAsync(GetKeyProcessTimer(processId, x)).ConfigureAwait(false)) }))
                .ConfigureAwait(false)).ToList();

            //var result = new List<ProcessTimer>(timerIds.Count());

            var times = (await db.SortedSetRangeByRankWithScoresAsync(GetKeyTimerTime()).ConfigureAwait(false))
                .Where(rv => rv.Element.HasValue)
                .Select(t => new {Id = Guid.Parse(t.Element), Time = t.Score});

            if (excludeIgnored)
            {
                string keyTimerIgnore = GetKeyTimerIgnore();
                timerIds = timerIds.Where(t => !db.HashExists(keyTimerIgnore, t.Id.ToString("N"))).ToList();
            }

            return timerIds.Where(x=>times.Any(t => t.Id == x.Id)).Select(x =>
                new ProcessTimer(x.Id, x.Name, _runtime.ToRuntimeTime(new DateTime(1970, 1, 1).AddMilliseconds(times.First(t => t.Id == x.Id).Time)))).ToList();
        }

       public virtual async Task<DateTime?> GetNextTimerDateAsync(TimerCategory timerCategory, int timerInterval)
        {
            string timerCategoryName = timerCategory.ToString();

            IDatabase db = _connector.GetDatabase();

            RedisValue lockValue = await db.StringGetAsync(GetKeyForWorkflowSyncLocks(timerCategory)).ConfigureAwait(false);

            if (!lockValue.HasValue)
            {
                throw new Exception($"Sync lock {timerCategoryName} not found");
            }

            string sortedSetKey = GetKeyForWorkflowRuntimeTimer(timerCategoryName);

            DateTime result = _runtime.RuntimeDateTimeNow;

            int index = 0;

            while (true)
            {
                SortedSetEntry[] se = await db.SortedSetRangeByRankWithScoresAsync(sortedSetKey, index, index + 1, Order.Descending).ConfigureAwait(false);

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

                if (await db.SetContainsAsync(GetKeyForWorkflowRuntimeStatusSet(RuntimeStatus.Alive), runtimeId).ConfigureAwait(false))
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
#pragma warning disable 4014
            tran.SortedSetAddAsync(sortedSetKey, _runtime.Id, (result.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds);
            tran.StringSetAsync(GetKeyForWorkflowSyncLocks(timerCategory), newLock.ToString("N"));
#pragma warning restore 4014

            bool tranResult = await tran.ExecuteAsync().ConfigureAwait(false);

            if (!tranResult)
            {
                return null;
            }

            return result;
        }

       public virtual async Task<List<WorkflowRuntimeModel>> GetWorkflowRuntimesAsync()
        {
            IDatabase db = _connector.GetDatabase();
            RedisValue[] keys = await db.HashKeysAsync(GetKeyForWorkflowRuntimeStatuses()).ConfigureAwait(false);
            WorkflowRuntimeModel[] workflowRuntimeModels = await Task.WhenAll(keys.Select(async v => await GetWorkflowRuntimeStatusAsync(db, v.ToString()).ConfigureAwait(false))).ConfigureAwait(false);
            return workflowRuntimeModels.ToList();
        }

       public virtual async Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId)
        {
       
            IDatabase db = _connector.GetDatabase();
            
            var processIdsLists = (await db.ListRangeAsync(GetKeyForSubprocesses(rootProcessId)).ConfigureAwait(false)).Where(v => v.HasValue).Select(v => new Guid(v.ToString())).ToList();
            
            RedisValue[] redisProcesses = await Task.WhenAll(processIdsLists.Select(processId => db.StringGetAsync(GetKeyForProcessInstance(processId)))).ConfigureAwait(false);
            
            var processInfo =
                redisProcesses
                    .Where(v => v.HasValue)
                    .Select(v =>
                    {
                        WorkflowProcessInstance processInstance = JsonConvert.DeserializeObject<WorkflowProcessInstance>(v);
            
                        if (!processInstance.SchemeId.HasValue)
                        {
                            throw SchemeNotFoundException.Create(processInstance.Id, SchemeLocation.WorkflowProcessInstance);
                        }
            
                        return (processId: processInstance.Id, schemeId: processInstance.SchemeId.Value, parentProcessId: processInstance.ParentProcessId,
                            rootProcessId: processInstance.RootProcessId, subprocessName: processInstance.SubprocessName);
                    }).ToList();
            
            
            var startingTransitions = (await Task.WhenAll(processInfo.Select(i => i.schemeId).Distinct().Select(async schemeId =>
            {
                RedisValue s = await db.StringGetAsync(GetKeyForProcessScheme(schemeId)).ConfigureAwait(false);
                if (!s.HasValue)
                {
                    throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
                }
            
                WorkflowProcessScheme processScheme = JsonConvert.DeserializeObject<WorkflowProcessScheme>(s);
            
                return (schemeId, startingTransition: processScheme.StartingTransition);
            })).ConfigureAwait(false)).ToDictionary(t => t.schemeId, t => t.startingTransition);
            
            
            return ProcessInstanceTreeItem.Create(rootProcessId, processInfo, startingTransitions);
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

        #region IApprovalProvider

        public async Task InsertInboxAsync(List<InboxItem> inboxItems)
        {
            IDatabase db = _connector.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            IEnumerable<HashEntry> fields = inboxItems.Select(x => new HashEntry(x.Id.ToString(), JsonConvert.SerializeObject(WorkflowInbox.ToDB(x))));
            foreach (InboxItem item in inboxItems)
            {

#pragma warning disable 4014
                tran.HashSetAsync(GetKeyInboxByProcessId(item.ProcessId),
                    item.Id.ToString(),
                    item.IdentityId??String.Empty);
#pragma warning restore 4014

#pragma warning disable 4014
                tran.HashSetAsync(GetKeyInboxByIdentityId(item.IdentityId),
                    item.Id.ToString(),
                    item.ProcessId.ToString());
#pragma warning restore 4014
            }

            //add to Inbox HashSet 
#pragma warning disable 4014
            tran.HashSetAsync(GetKeyInbox(), fields.ToArray());
#pragma warning restore 4014
            
            await tran.ExecuteAsync().ConfigureAwait(false);
        }

        public async Task DropWorkflowInboxAsync(Guid processId)
        {
           await DropWorkflowInboxInternalAsync(processId).ConfigureAwait(false);
        }

        private async Task DropWorkflowInboxInternalAsync(Guid processId, ITransaction transaction = null)
       {
           IDatabase db = _connector.GetDatabase();
           ITransaction tran = transaction ?? db.CreateTransaction();
            
           string keyByProcessId = GetKeyInboxByProcessId(processId);
           //Get ids By ProcessId
           List<Guid> ids = await GetIdsAsync(db, keyByProcessId).ConfigureAwait(false);
           RedisValue[] idsRedisValues = ids.Select(x => (RedisValue)x.ToString()).ToArray();
           
           //Get inbox
           List<WorkflowInbox> inbox  = await GetInboxAsync(db, idsRedisValues).ConfigureAwait(false);
           foreach (WorkflowInbox item in inbox)
           {
               string id = item.Id.ToString();
               string keyByIdentityId = GetKeyInboxByIdentityId(item.IdentityId);

               //Delete from ProcessId HashSet 
#pragma warning disable 4014
               tran.HashDeleteAsync(keyByProcessId, id);
#pragma warning restore 4014

               //Delete from IdentityId HashSet
#pragma warning disable 4014
               tran.HashDeleteAsync(keyByIdentityId, id);
#pragma warning restore 4014
           }

           //Delete from Inbox HashSet 
#pragma warning disable 4014
           tran.HashDeleteAsync(GetKeyInbox(), idsRedisValues);
#pragma warning restore 4014

           if (transaction == null)
           {
               await tran.ExecuteAsync().ConfigureAwait(false);
           }
       }
        
       public async Task<int> GetInboxCountByProcessIdAsync(Guid processId)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyInboxByProcessId(processId);
           return (await GetIdsAsync(db, key).ConfigureAwait(false)).Count();
       }
       public async Task<int> GetInboxCountByIdentityIdAsync(string identityId)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyInboxByIdentityId(identityId);
           return (await GetIdsAsync(db, key).ConfigureAwait(false)).Count();
       }
       public async Task<List<InboxItem>> GetInboxByProcessIdAsync(Guid processId, Paging paging = null, CultureInfo culture = null)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyInboxByProcessId(processId);
           List<Guid> ids = await GetIdsAsync(db, key, paging, true).ConfigureAwait(false);
           List<WorkflowInbox> inboxItems = await GetInboxAsync(db, ids).ConfigureAwait(false);
           
           return await WorkflowInbox.FromDB(_runtime, inboxItems.OrderByDescending(x=>x.AddingDate).ToArray(), culture ?? CultureInfo.CurrentCulture)
               .ConfigureAwait(false);
       }
       public async Task<List<InboxItem>> GetInboxByIdentityIdAsync(string identityId, Paging paging = null, CultureInfo culture = null)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyInboxByIdentityId(identityId);
           List<Guid> ids = await GetIdsAsync(db, key, paging, true).ConfigureAwait(false);
           List<WorkflowInbox> inboxItems = await GetInboxAsync(db, ids).ConfigureAwait(false);
           
           return await WorkflowInbox.FromDB(_runtime, inboxItems.OrderByDescending(x=>x.AddingDate).ToArray(), culture ?? CultureInfo.CurrentCulture)
               .ConfigureAwait(false);
       }
       public async Task FillApprovalHistoryAsync(ApprovalHistoryItem approvalHistoryItem)
       {
           IDatabase db = _connector.GetDatabase();
           ITransaction tran = db.CreateTransaction();
           string processId = approvalHistoryItem.ProcessId.ToString();
           string identityId = approvalHistoryItem.IdentityId;
           string keyByProcessId = GetKeyApprovalHistoryByProcessId(approvalHistoryItem.ProcessId);
           string keyByIdentityId = GetKeyApprovalHistoryByIdentityId(approvalHistoryItem.IdentityId);
           
           List<Guid> ids = await GetIdsAsync(db, keyByProcessId).ConfigureAwait(false);
           List<WorkflowApprovalHistory> approvalHistories = await GetApprovalHistoryAsync(db, ids).ConfigureAwait(false);

           WorkflowApprovalHistory historyItem = approvalHistories.FirstOrDefault(h => !h.TransitionTime.HasValue &&
           h.InitialState == approvalHistoryItem.InitialState &&
           h.DestinationState == approvalHistoryItem.DestinationState);
           
           if (historyItem is null)
           {
               historyItem = WorkflowApprovalHistory.ToDB(approvalHistoryItem);
               
           }
           else
           {
               historyItem.TriggerName = approvalHistoryItem.TriggerName;
               historyItem.TransitionTime = approvalHistoryItem.TransitionTime;
               historyItem.IdentityId = approvalHistoryItem.IdentityId;
               historyItem.Commentary = approvalHistoryItem.Commentary;
           }
           
           string id = historyItem.Id.ToString();
           
           //add to ProcessId HashSet  
#pragma warning disable 4014
           tran.HashSetAsync(keyByProcessId, id, identityId??String.Empty);
#pragma warning restore 4014              
               
           //add to IdentityId HashSet 
#pragma warning disable 4014
           tran.HashSetAsync(keyByIdentityId, id, processId);
#pragma warning restore 4014
           
           string json = JsonConvert.SerializeObject(historyItem);
            //add to Inbox HashSet 
#pragma warning disable 4014
            tran.HashSetAsync(GetKeyApprovalHistory(), id,json);
#pragma warning restore 4014
            await tran.ExecuteAsync().ConfigureAwait(false);
       }
       public virtual async Task DropEmptyApprovalHistoryAsync(Guid processId)
        {
            IDatabase db = _connector.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            string keyByProcessId = GetKeyApprovalHistoryByProcessId(processId);
            List<Guid> ids = await GetIdsAsync(db, keyByProcessId).ConfigureAwait(false);
            RedisValue[] idsRedisValues = ids.Select(x => (RedisValue)x.ToString()).ToArray();
           
            //Get ApprovalHistory
            List<WorkflowApprovalHistory> approvalHistories  = await GetApprovalHistoryAsync(db, idsRedisValues).ConfigureAwait(false);
            var empty = approvalHistories.Where(x => !x.TransitionTime.HasValue);
            RedisValue[] emptyIds = empty.Select(x => (RedisValue)x.Id.ToString()).ToArray();
            foreach (WorkflowApprovalHistory item in empty)
            {
                string id = item.Id.ToString();
                string keyByIdentityId = GetKeyApprovalHistoryByIdentityId(item.IdentityId);
               
                //Delete from ProcessId HashSet 
#pragma warning disable 4014
                tran.HashDeleteAsync(keyByProcessId, id);
#pragma warning restore 4014
               
                //Delete from IdentityId HashSet
#pragma warning disable 4014
                tran.HashDeleteAsync(keyByIdentityId, id);
#pragma warning restore 4014
            }
           
            //Delete empty from ApprovalHistory HashSet 
#pragma warning disable 4014
            tran.HashDeleteAsync(GetKeyApprovalHistory(), emptyIds);
#pragma warning restore 4014
            await tran.ExecuteAsync().ConfigureAwait(false);
        }

       public async Task DropApprovalHistoryByProcessIdAsync(Guid processId)
       {
           await DropApprovalHistoryByProcessIdInternalAsync(processId).ConfigureAwait(false);
       }

       private async Task DropApprovalHistoryByProcessIdInternalAsync(Guid processId, ITransaction transaction = null)
       {
           IDatabase db = _connector.GetDatabase();
           ITransaction tran = transaction ?? db.CreateTransaction();
           
           string keyByProcessId = GetKeyApprovalHistoryByProcessId(processId);
           //Get ids By ProcessId
           List<Guid> ids = await GetIdsAsync(db, keyByProcessId).ConfigureAwait(false);
           RedisValue[] idsRedisValues = ids.Select(x => (RedisValue)x.ToString()).ToArray();
           
           //Get ApprovalHistory
           List<WorkflowApprovalHistory> approvalHistories  = await GetApprovalHistoryAsync(db, idsRedisValues).ConfigureAwait(false);
           foreach (WorkflowApprovalHistory item in approvalHistories)
           {
               string id = item.Id.ToString();
               string keyByIdentityId = GetKeyApprovalHistoryByIdentityId(item.IdentityId);
               
               //Delete from ProcessId HashSet 
#pragma warning disable 4014
               tran.HashDeleteAsync(keyByProcessId, id);
#pragma warning restore 4014
               
               //Delete from IdentityId HashSet
#pragma warning disable 4014
               tran.HashDeleteAsync(keyByIdentityId, id);
#pragma warning restore 4014
           }
           
#pragma warning disable 4014
           //Delete from ApprovalHistory HashSet 
           tran.HashDeleteAsync(GetKeyApprovalHistory(), idsRedisValues);
#pragma warning restore 4014

           if ((transaction == null))
           {
               await tran.ExecuteAsync().ConfigureAwait(false);
           }
       }
       public async Task DropApprovalHistoryByIdentityIdAsync(string identityId)
       {
           IDatabase db = _connector.GetDatabase();
           ITransaction tran = db.CreateTransaction();
           
           string keyByIdentityId = GetKeyApprovalHistoryByIdentityId(identityId);
           //Get ids By identityId
           List<Guid> ids = await GetIdsAsync(db, keyByIdentityId).ConfigureAwait(false);
           RedisValue[] idsRedisValues = ids.Select(x => (RedisValue)x.ToString()).ToArray();
           
           //Get ApprovalHistory
           List<WorkflowApprovalHistory> approvalHistories  = await GetApprovalHistoryAsync(db, idsRedisValues).ConfigureAwait(false);
           foreach (WorkflowApprovalHistory item in approvalHistories)
           {
               string id = item.Id.ToString();
               string keyByProcessId = GetKeyApprovalHistoryByProcessId(item.ProcessId);
               
               //Delete from ProcessId HashSet 
#pragma warning disable 4014
               tran.HashDeleteAsync(keyByProcessId, id);
#pragma warning restore 4014
               
               //Delete from IdentityId HashSet
#pragma warning disable 4014
               tran.HashDeleteAsync(keyByIdentityId, id);
#pragma warning restore 4014
           }
           
           //Delete from ApprovalHistory HashSet 
           await tran.HashDeleteAsync(GetKeyApprovalHistory(), idsRedisValues).ConfigureAwait(false);
           await tran.ExecuteAsync().ConfigureAwait(false);
       }
       public async Task<int> GetApprovalHistoryCountByProcessIdAsync(Guid processId)
       {
           IDatabase db = _connector.GetDatabase();
           string key =  GetKeyApprovalHistoryByProcessId(processId);
           return (await GetIdsAsync(db, key).ConfigureAwait(false)).Count();
       }
       public async Task<int> GetApprovalHistoryCountByIdentityIdAsync(string identityId)
       {
           IDatabase db = _connector.GetDatabase();
           string key =  GetKeyApprovalHistoryByIdentityId(identityId);
           return (await GetIdsAsync(db, key).ConfigureAwait(false)).Count();
       }
       public async Task<List<ApprovalHistoryItem>> GetApprovalHistoryByProcessIdAsync(Guid processId, Paging paging = null)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyApprovalHistoryByProcessId(processId);
           List<Guid> ids = await GetIdsAsync(db, key, paging).ConfigureAwait(false);
           List<WorkflowApprovalHistory> approvalHistories =
               await GetApprovalHistoryAsync(db, ids).ConfigureAwait(false);
           
           return approvalHistories.OrderBy(x=>x.Sort).Select(x=>WorkflowApprovalHistory.FromDB(_runtime, x)).ToList();
       }
       public async Task<List<ApprovalHistoryItem>> GetApprovalHistoryByIdentityIdAsync(string identityId, Paging paging = null)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyApprovalHistoryByIdentityId(identityId);
           List<Guid> ids = await GetIdsAsync(db, key, paging).ConfigureAwait(false);
           List<WorkflowApprovalHistory> approvalHistories = await GetApprovalHistoryAsync(db, ids).ConfigureAwait(false);
           return approvalHistories.OrderBy(x=>x.Sort).Select(x=>WorkflowApprovalHistory.FromDB(_runtime, x)).ToList();
       }
       public async Task<int> GetOutboxCountByIdentityIdAsync(string identityId)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyApprovalHistoryByIdentityId(identityId);
           Dictionary<Guid, Guid> pairs = await GetPairsByIdentityIdAsync(db, key).ConfigureAwait(false);
           IEnumerable<IGrouping<Guid, KeyValuePair<Guid, Guid>>> groups = pairs.GroupBy(x => x.Value);
           return groups.Count();
       }
       public async Task<List<OutboxItem>> GetOutboxByIdentityIdAsync(string identityId, Paging paging = null)
       {
           IDatabase db = _connector.GetDatabase();
           string key = GetKeyApprovalHistoryByIdentityId(identityId);
           Dictionary<Guid, Guid> pairs = await GetPairsByIdentityIdAsync(db, key).ConfigureAwait(false);
           var groups = new List<IGrouping<Guid, KeyValuePair<Guid, Guid>>>();
           
           if (paging == null)
           {
               groups.AddRange(pairs.GroupBy(x => x.Value));
           }
           else
           {
               groups.AddRange(pairs.GroupBy(x => x.Value)
                   .Skip(paging.SkipCount())
                   .Take(paging.PageSize));
           }
           
           var outboxItems = new List<OutboxItem>();
           
           foreach (IGrouping<Guid, KeyValuePair<Guid, Guid>> group in groups)
           {
               string keyByProcessId = GetKeyApprovalHistoryByProcessId(group.Key);
               
               Dictionary<Guid, string>  idsByProcessId = await GetPairsByProcessIdAsync(db, keyByProcessId)
                   .ConfigureAwait(false);

               var idsOutbox = idsByProcessId
                   .Where(x => x.Value == identityId)
                   .Select(x => x.Key).ToList();

               var outboxItem = new OutboxItem() {ApprovalCount = group.Count(), ProcessId = group.Key, LastApprovalTime = DateTime.MinValue, FirstApprovalTime = DateTime.MaxValue};
               
               List<WorkflowApprovalHistory> histories =  await GetApprovalHistoryAsync(db, idsOutbox).ConfigureAwait(false);
               
               foreach (WorkflowApprovalHistory item in histories)
               {
                   item.TransitionTime = _runtime.ToRuntimeTime(item.TransitionTime);

                   if (item.TransitionTime > outboxItem.LastApprovalTime)
                   {
                       outboxItem.LastApprovalTime = item.TransitionTime;
                       outboxItem.LastApproval = item.TriggerName;
                   }
                   
                   if (item.TransitionTime < outboxItem.FirstApprovalTime)
                   {
                       outboxItem.FirstApprovalTime = item.TransitionTime;
                   }
               }
               if(histories.Any())
               {
                   outboxItems.Add(outboxItem);
               }
           }

           return outboxItems.OrderByDescending(x => x.LastApprovalTime).ToList();
       }
       
       #region Internal

           private async Task<List<HashEntry>> HashGetWitPagingAsync(IDatabase db, string key, Paging paging = null, bool reverse = false)
           {
               var hashAll = new List<HashEntry>();
 
               if (paging == null)
               {
                   HashEntry[] result = await db.HashGetAllAsync(key).ConfigureAwait(false);
                   hashAll.AddRange(result);
               }
               else
               {
                   if (reverse)
                   {
                       hashAll.AddRange((await db.HashGetAllAsync(key)
                               .ConfigureAwait(false))
                           .Reverse()
                           .Skip(paging.SkipCount())
                           .Take(paging.PageSize));
                   }
                   else
                   {
                       hashAll.AddRange((await db.HashGetAllAsync(key)
                               .ConfigureAwait(false))
                           .Skip(paging.SkipCount())
                           .Take(paging.PageSize));
                   }
               }
               return hashAll;
           }
           private async Task<List<Guid>> GetIdsAsync(IDatabase db, string key, Paging paging = null, bool reverse = false)
           {
               var hashAll = await HashGetWitPagingAsync(db, key, paging, reverse).ConfigureAwait(false);
               return hashAll.Select(x =>  new Guid(x.Name.ToString())).ToList();
           }
           private async Task<Dictionary<Guid, string>> GetPairsByProcessIdAsync(IDatabase db, string key, Paging paging = null, bool reverse = false)
           {
               var hashAll = await HashGetWitPagingAsync(db, key, paging, reverse).ConfigureAwait(false);
                return hashAll.ToDictionary(x =>  new Guid(x.Name.ToString()), x => x.Value.ToString());
           }
           private async Task<Dictionary<Guid, Guid>> GetPairsByIdentityIdAsync(IDatabase db, string key, Paging paging = null, bool reverse = false)
           {
               var hashAll = await HashGetWitPagingAsync(db, key, paging, reverse).ConfigureAwait(false);
               return hashAll.ToDictionary(x =>  new Guid(x.Name.ToString()), x => new Guid(x.Value.ToString()));
           }
           private async Task<List<WorkflowInbox>> GetInboxAsync(IDatabase db,List<Guid> ids)
           {
               RedisValue[] keysInbox = ids.Select(x => (RedisValue)x.ToString()).ToArray();
               return await GetInboxAsync(db, keysInbox).ConfigureAwait(false);
           }
           private async Task<List<WorkflowInbox>> GetInboxAsync(IDatabase db, RedisValue [] ids)
           {
               string key = GetKeyInbox();
               RedisValue[] values = await db.HashGetAsync(key, ids).ConfigureAwait(false);
               return values.Select(x=>JsonConvert.DeserializeObject<WorkflowInbox>(x)).ToList();
           }
           private async Task<List<WorkflowApprovalHistory>> GetApprovalHistoryAsync(IDatabase db,List<Guid> ids)
           {
               RedisValue[] keysInbox = ids.Select(x => (RedisValue)x.ToString()).ToArray();
               return await GetApprovalHistoryAsync(db, keysInbox).ConfigureAwait(false);
           }
           private async Task<List<WorkflowApprovalHistory>> GetApprovalHistoryAsync(IDatabase db, RedisValue [] ids)
           {
               string key = GetKeyApprovalHistory();
               RedisValue[] values = await db.HashGetAsync(key, ids).ConfigureAwait(false);
               return values.Select(x => JsonConvert.DeserializeObject<WorkflowApprovalHistory>(x)).ToList();
           }

       #endregion Internal

       
       #endregion IApprovalProvider
    }
}
