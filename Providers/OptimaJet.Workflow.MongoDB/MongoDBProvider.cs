using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Runtime.Timers;
using OptimaJet.Workflow.Core.Helpers;
using SortDirection = OptimaJet.Workflow.Core.Persistence.SortDirection;

namespace OptimaJet.Workflow.MongoDB
{
    public static class MongoDBConstants
    {
        public const string WorkflowProcessInstanceCollectionName = "WorkflowProcessInstance";
        public const string WorkflowProcessSchemeCollectionName = "WorkflowProcessScheme";
        public const string WorkflowProcessTransitionHistoryCollectionName = "WorkflowProcessTransitionHistory";
        public const string WorkflowSchemeCollectionName = "WorkflowScheme";
        public const string WorkflowProcessTimerCollectionName = "WorkflowProcessTimer";
        public const string WorkflowGlobalParameterCollectionName = "WorkflowGlobalParameter";
        public const string WorkflowRuntimeCollectionName = "WorkflowRuntime";
        public const string WorkflowSyncCollectionName = "WorkflowSync";
    
        public const string WorkflowApprovalHistoryCollectionName = "WorkflowApprovalHistory";
        public const string WorkflowInboxCollectionName = "WorkflowInbox";
    }

    public class MongoDBProvider : IWorkflowProvider
    {
        private WorkflowRuntime _runtime;
        private readonly bool _writeToHistory;
        private readonly bool _writeSubProcessToRoot;

        public MongoDBProvider(IMongoDatabase store,bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            Store = store;
            _writeToHistory = writeToHistory;
            _writeSubProcessToRoot = writeSubProcessToRoot;
        }

        public IMongoDatabase Store { get; set; }

        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
            CheckInitialData();
        }

        private SchemeDefinition<XElement> ConvertToSchemeDefinition(WorkflowProcessScheme workflowProcessScheme)
        {
            return new SchemeDefinition<XElement>(workflowProcessScheme.Id, workflowProcessScheme.RootSchemeId,
                workflowProcessScheme.SchemeCode, workflowProcessScheme.RootSchemeCode,
                XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete, false,
                workflowProcessScheme.AllowedActivities, workflowProcessScheme.StartingTransition,
                workflowProcessScheme.DefiningParameters);
        }

        #region IPersistenceProvider
       public virtual async Task DeleteInactiveTimersByProcessIdAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessTimer> dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            await dbcollTimer.DeleteManyAsync(c => c.ProcessId == processId && c.Ignore).ConfigureAwait(false);
        }

       public virtual async Task DeleteTimerAsync(Guid timerId)
        {
            IMongoCollection<WorkflowProcessTimer> dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            await dbcollTimer.DeleteOneAsync(x => x.Id == timerId).ConfigureAwait(false);
        }

       public virtual async Task<List<Guid>> GetRunningProcessesAsync(string runtimeId = null)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

            ProjectionDefinition<WorkflowProcessInstance> projection = Builders<WorkflowProcessInstance>.Projection
                .Include(b => b.Id);
            
            var options = new FindOptions<WorkflowProcessInstance, BsonDocument> {Projection = projection};
            
            FilterDefinition<WorkflowProcessInstance> filter = Builders<WorkflowProcessInstance>.Filter.Eq(n => n.Status.Status, ProcessStatus.Running.Id);
            
            if(String.IsNullOrEmpty(runtimeId))
            {
                return (await (await dbcoll.FindAsync(filter,options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(x => x.GetValue("_id").AsGuid).ToList();
            }

            var filters = new List<FilterDefinition<WorkflowProcessInstance>> {filter, Builders<WorkflowProcessInstance>.Filter.Eq(n => n.Status.RuntimeId, runtimeId)};

            FilterDefinition<WorkflowProcessInstance> combinedFilter = Builders<WorkflowProcessInstance>.Filter.And(filters);
            
            return (await (await dbcoll.FindAsync(combinedFilter,options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(x => x.GetValue("_id").AsGuid).ToList();

            //return (await dbcoll.FindAsync(x => x.Status.Status == ProcessStatus.Running.Id && x.Status.RuntimeId == runtimeId).ConfigureAwait(false)).ToList().Select(x => x.Id).ToList();
        }

       public virtual async Task<WorkflowRuntimeModel> CreateWorkflowRuntimeAsync(string runtimeId, RuntimeStatus status)
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll = 
                Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            var runtime = new Models.WorkflowRuntime()
            {
                RuntimeId = runtimeId,
                Lock = Guid.NewGuid(),
                Status = status
            };

            await dbcoll.InsertOneAsync(runtime).ConfigureAwait(false);

            return new WorkflowRuntimeModel { Lock = runtime.Lock, RuntimeId = runtimeId, Status = status };
            
        }

       public virtual async Task DeleteWorkflowRuntimeAsync(string name)
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll =
                 Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);
            await dbcoll.DeleteOneAsync(x => x.RuntimeId == name).ConfigureAwait(false);
        }

       public virtual async Task<List<ProcessInstanceItem>> GetProcessInstancesAsync(List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
       {
           IMongoCollection<WorkflowProcessInstance> workflowProcessInstanceCollection =
               Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

           
           List<WorkflowProcessInstance> processInstances;
           
           if (paging is null || orderParameters is null)
           {
               processInstances =
                   await workflowProcessInstanceCollection.AsQueryable().ToListAsync().ConfigureAwait(false);
           }
           else
           {
               processInstances = workflowProcessInstanceCollection.AsQueryable()
                   .OrderBy(GetOrderParameters(orderParameters))
                   .Skip(paging.SkipCount())
                   .Take(paging.PageSize)
                   .ToList();
           }
           
           IEnumerable<Guid?> schemeIds = processInstances.Select(x => x.SchemeId);
           IMongoCollection<WorkflowProcessScheme> workflowProcessSchemeCollection =
               Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
           ProjectionDefinition<WorkflowProcessScheme> workflowProcessSchemeProjection = Builders<WorkflowProcessScheme>.Projection
               .Include(ps => ps.Id)
               .Include(ps => ps.StartingTransition);

           var workflowProcessSchemeOptions = new FindOptions<WorkflowProcessScheme, BsonDocument> {Projection = workflowProcessSchemeProjection};

           FilterDefinition<WorkflowProcessScheme> workflowProcessSchemeFilter = Builders<WorkflowProcessScheme>.Filter.In(ps => ps.Id, schemeIds);
           
           List<BsonDocument> schemes =
               await (await workflowProcessSchemeCollection.FindAsync(workflowProcessSchemeFilter, workflowProcessSchemeOptions).ConfigureAwait(false))
                   .ToListAsync().ConfigureAwait(false);

           return processInstances.Join(
               schemes,
               pi => pi.SchemeId,
               s => s["_id"].AsGuid,
               (pi, s) => new ProcessInstanceItem()
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
                   SubprocessName  = pi.SubprocessName,
                   CreationDate  = pi.CreationDate,
                   LastTransitionDate  = pi.LastTransitionDate,
                   StartingTransition = s[nameof(WorkflowProcessScheme.StartingTransition)] == BsonNull.Value ? null : s[nameof(WorkflowProcessScheme.StartingTransition)].AsString
               }).ToList();
           
 
       }

       public virtual async Task<int> GetProcessInstancesCountAsync()
       {
           IMongoCollection<WorkflowProcessInstance> workflowProcessInstanceCollection =
               Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
           ConfiguredTaskAwaitable<long> count = workflowProcessInstanceCollection.CountDocumentsAsync(_ => true).ConfigureAwait(false);
           return Convert.ToInt32(count);
       }

       public virtual async Task<List<SchemeItem>> GetSchemesAsync(List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
       {
           IMongoCollection<WorkflowScheme> dbcoll =
               Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
           
           orderParameters ??= new List<(string parameterName, SortDirection sortDirection)>();
           IQueryable<WorkflowScheme> schemes = dbcoll.AsQueryable();

           //default sort for paging
           if ((paging != null)&&(orderParameters.Count<1))
           {
               orderParameters.Add((nameof(WorkflowScheme.Id),SortDirection.Asc));
           }

           if (orderParameters.Any())
           {
               schemes = schemes.OrderBy(GetOrderParameters(orderParameters));
           }
           
           if (paging != null)
           {
               schemes = schemes.Skip(paging.SkipCount()).Take(paging.PageSize);
           }
           
           
           return schemes.ToList().Select(sc => new SchemeItem()
           {
               Code = sc.Code,
               Scheme = sc.Scheme,
               CanBeInlined = sc.CanBeInlined,
               InlinedSchemes = sc.InlinedSchemes,
               Tags = sc.Tags,
           }).ToList();
       }

       public virtual async Task<int> GetSchemesCountAsync()
       {
           IMongoCollection<WorkflowScheme> dbcoll =
               Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
           ConfiguredTaskAwaitable<long> count = dbcoll.CountDocumentsAsync(_ => true).ConfigureAwait(false);
           return Convert.ToInt32(count);
       }
       
       public virtual async Task<WorkflowRuntimeModel> UpdateWorkflowRuntimeStatusAsync(WorkflowRuntimeModel runtime, RuntimeStatus status)
        {
            Tuple<long, WorkflowRuntimeModel> res = await UpdateWorkflowRuntimeAsync(runtime, x => x.Status = status, Builders<Models.WorkflowRuntime>.Update.Set(x => x.Status, status)).ConfigureAwait(false);

            if (res.Item1 != 1)
            {
                throw new ImpossibleToSetRuntimeStatusException();
            }

            return res.Item2;
        }

       public virtual async Task<(bool Success, WorkflowRuntimeModel UpdatedModel)> UpdateWorkflowRuntimeRestorerAsync(WorkflowRuntimeModel runtime, string restorerId)
        {
            Tuple<long, WorkflowRuntimeModel> res = await UpdateWorkflowRuntimeAsync(runtime, x => x.RestorerId = restorerId, Builders<Models.WorkflowRuntime>.Update.Set(x => x.RestorerId, restorerId))
                .ConfigureAwait(false);

            return (res.Item1 == 1, res.Item2);
        }

       public virtual async Task<bool> MultiServerRuntimesExistAsync()
        {
            string empty = Guid.Empty.ToString();
            IMongoCollection<Models.WorkflowRuntime> dbcoll = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);
            return await dbcoll
                .CountDocumentsAsync(x => x.RuntimeId != empty && x.Status != RuntimeStatus.Terminated && x.Status != RuntimeStatus.Dead)
                .ConfigureAwait(false) != 0;
        }

       public virtual async Task<int> ActiveMultiServerRuntimesCountAsync(string currentRuntimeId)
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);
            return (int)await dbcoll
                .CountDocumentsAsync(x => x.RuntimeId != currentRuntimeId && (x.Status == RuntimeStatus.Alive || x.Status == RuntimeStatus.Restore || x.Status == RuntimeStatus.SelfRestore))
                .ConfigureAwait(false);
        }

       public virtual async Task<WorkflowRuntimeModel> GetWorkflowRuntimeModelAsync(string runtimeId)
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);
            Models.WorkflowRuntime result = await (await dbcoll.FindAsync(x => x.RuntimeId == runtimeId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (result == null)
            {
                return null;
            }

            return GetModel(result);
        }

       public virtual async Task InitializeProcessAsync(ProcessInstance processInstance)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            long oldProcessCount = await dbcoll.CountDocumentsAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false);
                
            if (oldProcessCount != 0)
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
                Persistence = new List<WorkflowProcessInstancePersistence>(),
                TenantId = processInstance.TenantId,
                SubprocessName = processInstance.SubprocessName,
                CreationDate = processInstance.CreationDate
            };
            await dbcoll.InsertOneAsync(newProcess).ConfigureAwait(false);
        }

       public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance)
        {
            await BindProcessToNewSchemeAsync(processInstance, false).ConfigureAwait(false);
        }

       public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance oldProcess = await (await dbcoll.FindAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (oldProcess == null)
            {
                throw new ProcessNotFoundException(processInstance.ProcessId);
            }

            oldProcess.SchemeId = processInstance.SchemeId;
            if (resetIsDeterminingParametersChanged)
            {
                oldProcess.IsDeterminingParametersChanged = false;
            }

            await SaveAsync(dbcoll, oldProcess, doc => doc.Id == oldProcess.Id).ConfigureAwait(false);
        }

        private async Task SaveAsync<T>(IMongoCollection<T> collection, T obj, Expression<Func<T, bool>> filter)
        {
#if !NETCOREAPP
            await collection.ReplaceOneAsync<T>(filter, obj,new UpdateOptions() { IsUpsert = true }).ConfigureAwait(false);
#else
            await collection.ReplaceOneAsync<T>(filter, obj, new ReplaceOptions() { IsUpsert = true }).ConfigureAwait(false);
#endif
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

            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance process = await (await dbcoll.FindAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            
            if (process != null)
            {
                if (process.Persistence == null)
                {
                    process.Persistence = new List<WorkflowProcessInstancePersistence>();
                }
                
                var persistedParameters = process.Persistence.ToList();

                foreach (dynamic parameterDefinitionWithValue in parametersToPersistList)
                {
                    WorkflowProcessInstancePersistence persistence = persistedParameters.SingleOrDefault(pp => pp.ParameterName == parameterDefinitionWithValue.Parameter.Name);

                    InsertOrUpdateParameter(parameterDefinitionWithValue, process, persistence);
                }

                await SaveAsync(dbcoll, process, doc => doc.Id == process.Id).ConfigureAwait(false);
            }
        }
       public virtual async Task SavePersistenceParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            dynamic parameter = ParameterDefinitionWithValueToDynamic(processInstance.ProcessParameters.Single(ptp => ptp.Purpose == ParameterPurpose.Persistence && ptp.Name == parameterName));
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance process = await (await dbcoll.FindAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (process != null)
            {
                if (process.Persistence == null)
                {
                    process.Persistence = new List<WorkflowProcessInstancePersistence>();
                }
                
                WorkflowProcessInstancePersistence persistence = process.Persistence.SingleOrDefault(pp => pp.ParameterName == parameter.Parameter.Name);
                InsertOrUpdateParameter(parameter, process, persistence);
                await SaveAsync(dbcoll, process, doc => doc.Id == process.Id).ConfigureAwait(false);
            }

        }
        private dynamic ParameterDefinitionWithValueToDynamic(ParameterDefinitionWithValue ptp)
        {
            string serializedValue = ptp.Type == typeof(UnknownParameterType) ? (string)ptp.Value : ParametersSerializer.Serialize(ptp.Value, ptp.Type);
            return new { Parameter = ptp, SerializedValue = serializedValue };
        }
        private void InsertOrUpdateParameter(dynamic parameter, WorkflowProcessInstance process, WorkflowProcessInstancePersistence workflowProcessInstancePersistence)
        {
            if (workflowProcessInstancePersistence == null)
            {
                if (parameter.SerializedValue != null)
                {
                    workflowProcessInstancePersistence = new WorkflowProcessInstancePersistence
                    {
                        ParameterName = parameter.Parameter.Name,
                        Value = parameter.SerializedValue
                    };
                    process.Persistence.Add(workflowProcessInstancePersistence);
                }
            }
            else
            {
                if (parameter.SerializedValue != null)
                {
                    workflowProcessInstancePersistence.Value = parameter.SerializedValue;
                }
                else
                {
                    process.Persistence.Remove(workflowProcessInstancePersistence);
                }
            }
        }
       public virtual async Task RemoveParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance process = await (await dbcoll.FindAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (process?.Persistence != null)
            {
                WorkflowProcessInstancePersistence persistence = process.Persistence.SingleOrDefault(pp => pp.ParameterName == parameterName);
                process.Persistence.Remove(persistence);

                await SaveAsync(dbcoll, process, doc => doc.Id == process.Id).ConfigureAwait(false);
            }
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
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance instance = await (await dbcoll.FindAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            var status = new WorkflowProcessInstanceStatus
            {
                Lock = Guid.NewGuid(),
                Status = ProcessStatus.Initialized.Id,
                RuntimeId = _runtime.Id,
                SetTime = _runtime.RuntimeDateTimeNow
            };

            if (instance.Status == null)
            {
                await dbcoll.UpdateOneAsync(x => x.Id == instance.Id, Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, status)).ConfigureAwait(false);
            }
            else
            {
                Guid oldLock = instance.Status.Lock;

                UpdateResult result = await dbcoll.UpdateOneAsync(x => x.Id == instance.Id && x.Status.Lock == oldLock, Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, status))
                    .ConfigureAwait(false);

                if(result.ModifiedCount != 1)
                {
                    throw new ImpossibleToSetStatusException();
                }
            }
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

           IMongoCollection<WorkflowProcessTransitionHistory> dbcollTransition = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
           await dbcollTransition.InsertOneAsync(history).ConfigureAwait(false);
       }
       public virtual async Task UpdatePersistenceStateAsync(ProcessInstance processInstance, TransitionDefinition transition)
        {
            DateTime startTransitionTime = processInstance.StartTransitionTime ?? _runtime.RuntimeDateTimeNow;
            
            ParameterDefinitionWithValue paramIdentityId = await processInstance.GetParameterAsync(DefaultDefinitions.ParameterIdentityId.Name).ConfigureAwait(false);
            ParameterDefinitionWithValue paramImpIdentityId = await processInstance.GetParameterAsync(DefaultDefinitions.ParameterImpersonatedIdentityId.Name).ConfigureAwait(false);

            string identityId = paramIdentityId == null ? String.Empty : (string) paramIdentityId.Value;
            string impIdentityId = paramImpIdentityId == null ? identityId : (string) paramImpIdentityId.Value;

            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance inst = await (await dbcoll.FindAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
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

                await SaveAsync(dbcoll, inst, doc => doc.Id == inst.Id).ConfigureAwait(false);
            }

            if (!_writeToHistory)
            {
                return;
            }

            var history = new WorkflowProcessTransitionHistory
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

            IMongoCollection<WorkflowProcessTransitionHistory> dbcollTransition = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            await dbcollTransition.InsertOneAsync(history).ConfigureAwait(false);
        }
       public virtual async Task<bool> IsProcessExistsAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            return await dbcoll.CountDocumentsAsync(x => x.Id == processId).ConfigureAwait(false) != 0;
        }
       public virtual async Task<ProcessStatus> GetInstanceStatusAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance instance = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (instance == null)
            {
                return ProcessStatus.NotFound;
            }

            ProcessStatus status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status?.Status);
            if (status == null)
            {
                return ProcessStatus.Unknown;
            }

            return status;
        }
       private async Task SetCustomStatusAsync(Guid processId, ProcessStatus status)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance instance = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (instance == null)
            {
                throw new StatusNotDefinedException();
            }

            var newStatus = new WorkflowProcessInstanceStatus
            {
                Lock = Guid.NewGuid(),
                Status = status.Id,
                SetTime = _runtime.RuntimeDateTimeNow,
                RuntimeId = _runtime.Id
            };

            if (instance.Status == null)
            {
                await dbcoll.UpdateOneAsync(x => x.Id == instance.Id, Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, newStatus)).ConfigureAwait(false);
            }
            else
            {
                Guid oldLock = instance.Status.Lock;

                UpdateResult result = await dbcoll.UpdateOneAsync(x => x.Id == instance.Id && x.Status.Lock == oldLock, Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, newStatus))
                    .ConfigureAwait(false);

                if (result.ModifiedCount == 0)
                {
                    long cnt = await dbcoll.CountDocumentsAsync(x => x.Id == processId).ConfigureAwait(false);
                    if (cnt == 0)
                    {
                        throw new StatusNotDefinedException();
                    }
                }

                if (result.ModifiedCount != 1)
                {
                    throw new ImpossibleToSetStatusException();
                }
            }
        }
       private async Task SetRunningStatusAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance instance = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (instance?.Status == null)
            {
                throw new StatusNotDefinedException();
            }

            if (instance.Status.Status == ProcessStatus.Running.Id)
            {
                throw new ImpossibleToSetStatusException("Process already running");
            }

            var status = new WorkflowProcessInstanceStatus
            {
                Lock = Guid.NewGuid(),
                Status = ProcessStatus.Running.Id,
                SetTime = _runtime.RuntimeDateTimeNow,
                RuntimeId = _runtime.Id
            };

            Guid oldLock = instance.Status.Lock;

            UpdateResult result = await dbcoll.UpdateOneAsync(x => x.Id == instance.Id && x.Status.Lock == oldLock,
                Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, status)).ConfigureAwait(false);
            
            if (result.ModifiedCount == 0)
            {
                long cnt = await dbcoll.CountDocumentsAsync(x => x.Id == processId).ConfigureAwait(false);
                if (cnt == 0)
                {
                    throw new StatusNotDefinedException();
                }
            }

            if (result.ModifiedCount != 1)
            {
                throw new ImpossibleToSetStatusException();
            }
        }
       private async Task<IEnumerable<ParameterDefinitionWithValue>> GetProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
        {
            var parameters = new List<ParameterDefinitionWithValue>(processDefinition.Parameters.Count);
            parameters.AddRange(await GetPersistedProcessParametersAsync(processId, processDefinition).ConfigureAwait(false));
            parameters.AddRange(await GetSystemProcessParametersAsync(processId, processDefinition).ConfigureAwait(false));
            return parameters;
        }
       private async Task<IEnumerable<ParameterDefinitionWithValue>> GetSystemProcessParametersAsync(Guid processId,
            ProcessDefinition processDefinition)
        {
            WorkflowProcessInstance processInstance = await GetProcessInstanceAsync(processId).ConfigureAwait(false);

            var systemParameters = processDefinition.Parameters.Where(p => p.Purpose == ParameterPurpose.System).ToList();

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
                    _runtime.ToRuntimeTime(processInstance.CreationDate)),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterLastTransitionDate.Name),
                     _runtime.ToRuntimeTime(processInstance.LastTransitionDate))
            };
            return parameters;
        }

        private async Task<IEnumerable<ParameterDefinitionWithValue>> GetPersistedProcessParametersAsync(Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count);

            List<WorkflowProcessInstancePersistence> persistedParameters;
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance process = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (process?.Persistence != null)
            {
                persistedParameters = process.Persistence.ToList();
            }
            else
            {
                return parameters;
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
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance process = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (process?.Persistence != null)
            {
                persistedParameter = process.Persistence.FirstOrDefault(x => x.ParameterName == parameterName);
            }
            else
            {
                return null;
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
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

            WorkflowProcessInstance processInstance = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
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
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            WorkflowGlobalParameter parameter = await (await dbcoll.FindAsync(item => item.Type == type && item.Name == name).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (parameter == null)
            {
                parameter = new WorkflowGlobalParameter
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Type = type,
                    Value = JsonConvert.SerializeObject(value)
                };

                await dbcoll.InsertOneAsync(parameter).ConfigureAwait(false);
            }
            else
            {
                parameter.Value = JsonConvert.SerializeObject(value);
                await SaveAsync(dbcoll, parameter, doc => doc.Id == parameter.Id).ConfigureAwait(false);
            }
        }

       public virtual async Task<T> LoadGlobalParameterAsync<T>(string type, string name)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            WorkflowGlobalParameter parameter = await (await dbcoll.FindAsync(item => item.Type == type && item.Name == name).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (parameter != null)
            {
                return JsonConvert.DeserializeObject<T>(parameter.Value);
            }

            return default;
        }

       public virtual async Task<List<T>> LoadGlobalParametersAsync<T>(string type)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);
            
            return (await (await dbcoll.FindAsync(item => item.Type == type).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false))
                    .Select(gp => JsonConvert.DeserializeObject<T>(gp.Value))
                    .ToList();
        }

       public virtual async Task DeleteGlobalParametersAsync(string type, string name = null)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            Expression<Func<WorkflowGlobalParameter, bool>> predicate;

            if(String.IsNullOrEmpty(name))
            {
                predicate = item => item.Type == type;
            }
            else
            {
                predicate = item => item.Type == type && item.Name == name;
            }

            await dbcoll.DeleteManyAsync(predicate).ConfigureAwait(false);
        }

       public virtual async Task DeleteProcessAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessInstance> dbcollInstance = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            await dbcollInstance.DeleteOneAsync(c => c.Id == processId).ConfigureAwait(false);

            IMongoCollection<WorkflowProcessTransitionHistory> dbcollTransition = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            await dbcollTransition.DeleteManyAsync(c => c.ProcessId == processId).ConfigureAwait(false);

            IMongoCollection<WorkflowProcessTimer> dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            await dbcollTimer.DeleteManyAsync(c => c.ProcessId == processId).ConfigureAwait(false);
            
            IMongoCollection<WorkflowInbox> dbcollInbox = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            await dbcollInbox.DeleteManyAsync(c => c.ProcessId == processId).ConfigureAwait(false);
            
            IMongoCollection<WorkflowApprovalHistory> dbcollApprovalHisory = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            await dbcollApprovalHisory.DeleteManyAsync(c => c.ProcessId == processId).ConfigureAwait(false);
        }

       public virtual async Task RegisterTimerAsync(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            IMongoCollection<WorkflowProcessTimer> dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            WorkflowProcessTimer timer = await (await dbcoll.FindAsync(item => item.ProcessId == processId && item.Name == name).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
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

                await dbcoll.InsertOneAsync(timer).ConfigureAwait(false);
            }
            else if (!notOverrideIfExists)
            {
                timer.NextExecutionDateTime = nextExecutionDateTime;
                await SaveAsync(dbcoll, timer, doc => doc.Id == timer.Id).ConfigureAwait(false);
            }
        }

       public virtual async Task ClearTimersAsync(Guid processId, List<string> timersIgnoreList)
        {
            IMongoCollection<WorkflowProcessTimer> dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            await dbcollTimer.DeleteManyAsync(c => c.ProcessId == processId && !timersIgnoreList.Contains(c.Name)).ConfigureAwait(false);
        }

       public virtual async Task<int> SetTimerIgnoreAsync(Guid timerId)
        {
            IMongoCollection<WorkflowProcessTimer> dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            UpdateResult result = await dbcoll.UpdateManyAsync(item => item.Id == timerId && !item.Ignore, Builders<WorkflowProcessTimer>.Update.Set(c => c.Ignore, true)).ConfigureAwait(false);
            return (int)result.ModifiedCount;
        }

       public virtual async Task<List<Core.Model.WorkflowTimer>> GetTopTimersToExecuteAsync(int top)
        {
            DateTime now = _runtime.RuntimeDateTimeNow.ToUniversalTime();

            IMongoCollection<WorkflowProcessTimer> timerColl = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);

            IEnumerable<Core.Model.WorkflowTimer> result = 
                (await (await timerColl.FindAsync(x => !x.Ignore && x.NextExecutionDateTime <= now).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false))
                .Select(x => new Core.Model.WorkflowTimer
                {
                    Name = x.Name,
                    ProcessId = x.ProcessId,
                    TimerId = x.Id,
                    NextExecutionDateTime = _runtime.ToRuntimeTime(x.NextExecutionDateTime),
                    RootProcessId = x.RootProcessId
                });

            return result.ToList();
        }

       public virtual async Task<List<ProcessHistoryItem>> GetProcessHistoryAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessTransitionHistory> dbcoll = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            List<WorkflowProcessTransitionHistory> history = await (await dbcoll.FindAsync(hi => hi.ProcessId == processId).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
            return history.Select(hi => new ProcessHistoryItem
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
                    TransitionTime = _runtime.ToRuntimeTime(hi.TransitionTime),
                    TriggerName = hi.TriggerName,
                    StartTransitionTime = hi.StartTransitionTime,
                    TransitionDuration = hi.TransitionDuration
                })
                .ToList();
        }

       public virtual async Task<List<ProcessTimer>> GetTimersForProcessAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessTimer> dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            List<WorkflowProcessTimer> timers = await (await dbcoll.FindAsync(t => t.ProcessId == processId).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
            return timers.Select(t => new ProcessTimer(t.Id, t.Name, _runtime.ToRuntimeTime(t.NextExecutionDateTime))).ToList();
        }

       public virtual async Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId)
        {
            IMongoCollection<WorkflowProcessInstance> workflowProcessInstanceCollection =
                Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

            ProjectionDefinition<WorkflowProcessInstance> workflowProcessInstanceProjection = Builders<WorkflowProcessInstance>.Projection
                .Include(pi => pi.Id)
                .Include(pi => pi.ParentProcessId)
                .Include(pi => pi.RootProcessId)
                .Include(pi => pi.SchemeId)
                .Include(pi=>pi.SubprocessName);

            var workflowProcessInstanceOptions = new FindOptions<WorkflowProcessInstance, BsonDocument> {Projection = workflowProcessInstanceProjection};

            FilterDefinition<WorkflowProcessInstance> workflowProcessInstanceFilter =
                Builders<WorkflowProcessInstance>.Filter.Eq(pi => pi.RootProcessId, rootProcessId);

            List<BsonDocument> instances = await (await workflowProcessInstanceCollection.FindAsync(workflowProcessInstanceFilter, workflowProcessInstanceOptions)
                .ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
            var schemeIds = instances.Select(i => i[nameof(WorkflowProcessInstance.SchemeId)].AsGuid).Distinct().ToList();

            IMongoCollection<WorkflowProcessScheme> workflowProcessSchemeCollection =
                Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            ProjectionDefinition<WorkflowProcessScheme> workflowProcessSchemeProjection = Builders<WorkflowProcessScheme>.Projection
                .Include(ps => ps.Id)
                .Include(ps => ps.StartingTransition);

            var workflowProcessSchemeOptions = new FindOptions<WorkflowProcessScheme, BsonDocument> {Projection = workflowProcessSchemeProjection};

            FilterDefinition<WorkflowProcessScheme> workflowProcessSchemeFilter = Builders<WorkflowProcessScheme>.Filter.In(ps => ps.Id, schemeIds);
            List<BsonDocument> schemes =
                await (await workflowProcessSchemeCollection.FindAsync(workflowProcessSchemeFilter, workflowProcessSchemeOptions).ConfigureAwait(false))
                    .ToListAsync().ConfigureAwait(false);

            return ProcessInstanceTreeItem.CreateFromBsonDocuments(instances, schemes);
        }

       public virtual async Task<List<ProcessTimer>> GetActiveTimersForProcessAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessTimer> dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            List<WorkflowProcessTimer> timers = await (await dbcoll.FindAsync(t => t.ProcessId == processId && !t.Ignore).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
            return timers.Select(t => new ProcessTimer(t.Id, t.Name, _runtime.ToRuntimeTime(t.NextExecutionDateTime))).ToList();
        }

       public virtual async Task<int> SendRuntimeLastAliveSignalAsync()
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            DateTime time = _runtime.RuntimeDateTimeNow;
            string id = _runtime.Id;

            UpdateResult result = await dbcoll.UpdateOneAsync(x => (x.Status == RuntimeStatus.Alive || x.Status == RuntimeStatus.SelfRestore) && x.RuntimeId == id,
                Builders<Models.WorkflowRuntime>.Update.Set(x => x.LastAliveSignal, time)).ConfigureAwait(false);

            return (int)result.ModifiedCount;
        }

       public virtual async Task<DateTime?> GetNextTimerDateAsync(TimerCategory timerCategory, int timerInterval)
        {
            string timerCategoryName = timerCategory.ToString();
            IMongoCollection<Models.WorkflowSync> lockColl = Store.GetCollection<Models.WorkflowSync>(MongoDBConstants.WorkflowSyncCollectionName);
            Models.WorkflowSync sync = await (await lockColl.FindAsync(x => x.Name == timerCategoryName).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (sync == null)
            {
                throw new Exception($"Sync lock {timerCategoryName} not found");
            }

            Guid syncLock = sync.Lock;

            IMongoCollection<Models.WorkflowRuntime> runtimeColl = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            string runtimeId = _runtime.Id;

            Expression<Func<Models.WorkflowRuntime, object>> getterExpression;
            Func<Models.WorkflowRuntime, DateTime?> getterFunction;

            switch (timerCategory)
            {
                case TimerCategory.Timer:
                    getterExpression = e => e.NextTimerTime;
                    getterFunction = x => x.NextTimerTime;
                    break;
                case TimerCategory.ServiceTimer:
                    getterExpression = e => e.NextServiceTimerTime;
                    getterFunction = x => x.NextServiceTimerTime;
                    break;
                default:
                    throw new Exception($"Unknown sync lock name: {timerCategoryName}");
            }

            Models.WorkflowRuntime max =
                await (await runtimeColl.FindAsync(x => x.RuntimeId != runtimeId && x.Status == RuntimeStatus.Alive,
                        new FindOptions<Models.WorkflowRuntime>
                        {
                            Sort = Builders<Models.WorkflowRuntime>.Sort.Descending(getterExpression),
                            Limit = 1
                        })
                        .ConfigureAwait(false))
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

            DateTime result = _runtime.RuntimeDateTimeNow;

            DateTime? runtimeTime = max != null ? _runtime.ToRuntimeTime(getterFunction(max)) : null;
            
            if(runtimeTime != null && runtimeTime > result)
            {
                result = runtimeTime.Value;
            }

            result += TimeSpan.FromMilliseconds(timerInterval);

            using (IClientSessionHandle session = await Store.Client.StartSessionAsync().ConfigureAwait(false))
            {
                session.StartTransaction();

                var newLock = Guid.NewGuid();

                await runtimeColl.UpdateOneAsync(x => x.RuntimeId == runtimeId, Builders<Models.WorkflowRuntime>.Update.Set(getterExpression, result)).ConfigureAwait(false);

                UpdateResult lockUpdateResult = await lockColl.UpdateOneAsync(x => x.Lock == syncLock && x.Name == timerCategoryName, 
                    Builders<Models.WorkflowSync>.Update.Set(c => c.Lock, newLock)).ConfigureAwait(false);

                if(lockUpdateResult.ModifiedCount == 0)
                {
                    await session.AbortTransactionAsync().ConfigureAwait(false);

                    return null;
                }

                await session.CommitTransactionAsync().ConfigureAwait(false);
            }

            return result;
        }

       public virtual async Task<List<WorkflowRuntimeModel>> GetWorkflowRuntimesAsync()
        {
            IMongoCollection<Models.WorkflowRuntime> runtimeColl = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);
            return (await (await runtimeColl.FindAsync(Builders<Models.WorkflowRuntime>.Filter.Empty).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(GetModel).ToList();
        }
        private WorkflowRuntimeModel GetModel(Models.WorkflowRuntime result)
        {
            return new WorkflowRuntimeModel
            { 
                Lock = result.Lock, 
                RuntimeId = result.RuntimeId,
                Status = result.Status,
                RestorerId = result.RestorerId,
                LastAliveSignal = _runtime.ToRuntimeTime(result.LastAliveSignal),
                NextTimerTime = _runtime.ToRuntimeTime(result.NextTimerTime)
            };
        }
        

        #endregion

        #region ISchemePersistenceProvider

       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeByProcessIdAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

            WorkflowProcessInstance processInstance = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);


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
            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            WorkflowProcessScheme processScheme = await (await dbcoll.FindAsync(x => x.Id == schemeId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (processScheme == null || String.IsNullOrEmpty(processScheme.Scheme))
            {
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);
            }

            return ConvertToSchemeDefinition(processScheme);
        }


       public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeWithParametersAsync(string schemeCode, string definingParameters, Guid? rootSchemeId, bool ignoreObsolete)
        {
            string hash = HashHelper.GenerateStringHash(definingParameters);

            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            IEnumerable<WorkflowProcessScheme> processSchemes = ignoreObsolete
                ? await (await dbcoll.FindAsync(
                        pss =>
                            pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash &&
                            pss.RootSchemeId == rootSchemeId &&
                            !pss.IsObsolete).ConfigureAwait(false))
                    .ToListAsync().ConfigureAwait(false)
                : await (await dbcoll.FindAsync(
                        pss =>
                            pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash &&
                            pss.RootSchemeId == rootSchemeId).ConfigureAwait(false))
                    .ToListAsync().ConfigureAwait(false);

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

            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            await dbcoll.UpdateManyAsync(
                item => (item.SchemeCode == schemeCode || item.RootSchemeCode == schemeCode) && item.DefiningParametersHash == definingParametersHash,
                Builders<WorkflowProcessScheme>.Update.Set(c => c.IsObsolete, true)).ConfigureAwait(false);
        }

       public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode)
        {
            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            await dbcoll.UpdateManyAsync(item => item.SchemeCode == schemeCode || item.RootSchemeCode == schemeCode,
                Builders<WorkflowProcessScheme>.Update.Set(c => c.IsObsolete, true)).ConfigureAwait(false);
        }

       public virtual async Task<SchemeDefinition<XElement>> SaveSchemeAsync(SchemeDefinition<XElement> scheme)
        {
            string definingParameters = scheme.DefiningParameters;
            string definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            List<WorkflowProcessScheme> oldSchemes =
                await (await dbcoll.FindAsync(
                        wps => wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == scheme.SchemeCode &&
                               wps.IsObsolete == scheme.IsObsolete).ConfigureAwait(false))
                    .ToListAsync().ConfigureAwait(false);

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
                AllowedActivities = scheme.AllowedActivities,
                StartingTransition = scheme.StartingTransition,
                IsObsolete = scheme.IsObsolete
            };

            await dbcoll.InsertOneAsync(newProcessScheme).ConfigureAwait(false);

            return ConvertToSchemeDefinition(newProcessScheme);
        }

       public virtual async Task SaveSchemeAsync(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme wfScheme = await (await dbcoll.FindAsync(c => c.Code == schemaCode).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (wfScheme == null)
            {
                wfScheme = new WorkflowScheme
                {
                    Id = schemaCode,
                    Code = schemaCode,
                    Scheme = scheme,
                    InlinedSchemes = inlinedSchemes,
                    CanBeInlined = canBeInlined,
                    Tags = tags
                };
                await dbcoll.InsertOneAsync(wfScheme).ConfigureAwait(false);
            }
            else
            {
                wfScheme.Scheme = scheme;
                wfScheme.InlinedSchemes = inlinedSchemes;
                wfScheme.CanBeInlined = canBeInlined;
                wfScheme.Tags = tags;
                await SaveAsync(dbcoll, wfScheme, doc => doc.Id == wfScheme.Id).ConfigureAwait(false);
            }
        }

       public virtual async Task<XElement> GetSchemeAsync(string code)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme scheme = await (await dbcoll.FindAsync(c => c.Code == code).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (scheme == null || String.IsNullOrEmpty(scheme.Scheme))
            {
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);
            }

            return XElement.Parse(scheme.Scheme);
        }

       public virtual async Task<List<string>> GetInlinedSchemeCodesAsync()
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            FilterDefinition<WorkflowScheme> filter = Builders<WorkflowScheme>.Filter.Eq(n => n.CanBeInlined, true);
            ProjectionDefinition<WorkflowScheme> projection = Builders<WorkflowScheme>.Projection
                .Include(b => b.Code)
                .Exclude("_id");
            var options = new FindOptions<WorkflowScheme, BsonDocument> {Projection = projection};
            var codes = (await (await dbcoll.FindAsync(filter, options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString)
                .ToList();
            return codes;
        }

       public virtual async Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            FilterDefinition<WorkflowScheme> filter = Builders<WorkflowScheme>.Filter.AnyEq(sch => sch.InlinedSchemes, schemeCode);
            ProjectionDefinition<WorkflowScheme> projection = Builders<WorkflowScheme>.Projection
                .Include(b => b.Code)
                .Exclude("_id");
            var options = new FindOptions<WorkflowScheme, BsonDocument> {Projection = projection};
            var codes = (await (await dbcoll.FindAsync(filter, options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString)
                .ToList();
            return codes;
        }

       public virtual async Task<List<string>> SearchSchemesByTagsAsync(params string[] tags)
        {
            return await SearchSchemesByTagsAsync(tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags)
        {
            var tagsList = tags?.ToList();
            bool isEmpty = tagsList == null || !tagsList.Any();
            
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);

            var filters = new List<FilterDefinition<WorkflowScheme>>();

            ProjectionDefinition<WorkflowScheme> projection = Builders<WorkflowScheme>.Projection
                .Include(b => b.Code)
                .Exclude("_id");
            
            var options = new FindOptions<WorkflowScheme, BsonDocument> {Projection = projection};

            if (!isEmpty)
            {
                foreach (string tag in tagsList)
                {
                    filters.Add(Builders<WorkflowScheme>.Filter.AnyEq(s => s.Tags, tag));
                }

                return (await (await dbcoll.FindAsync(Builders<WorkflowScheme>.Filter.Or(filters), options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false))
                    .Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString).ToList();
            }

            return (await (await dbcoll.FindAsync(Builders<WorkflowScheme>.Filter.Empty, options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false))
                .Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString).ToList();
        }

       public virtual async Task AddSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await AddSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await UpdateSchemeTagsAsync(schemeCode, (schemeTags) => tags.Concat(schemeTags).ToList()).ConfigureAwait(false);
        }

       public virtual async Task RemoveSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await RemoveSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await UpdateSchemeTagsAsync(schemeCode,schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList()).ConfigureAwait(false);
        }

       public virtual async Task SetSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await SetSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

       public virtual async Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await UpdateSchemeTagsAsync(schemeCode, (schemeTags) => tags.ToList()).ConfigureAwait(false);
        }

        private async Task UpdateSchemeTagsAsync(string schemeCode, Func<List<string>, List<string>> getNewTags)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme scheme = await (await dbcoll.FindAsync(c => c.Code == schemeCode).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (scheme == null)
            {
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowScheme);
            }

            List<string> newTags = getNewTags.Invoke(scheme.Tags);

            scheme.Scheme = _runtime.Builder.ReplaceTagsInScheme(scheme.Scheme,newTags);
            scheme.Tags = newTags;

            await SaveAsync(dbcoll, scheme, doc => doc.Id == scheme.Id).ConfigureAwait(false);
        }

        #endregion

        #region IWorkflowGenerator

        private readonly IDictionary<string, string> _templateTypeMapping = new Dictionary<string, string>();

       public virtual async Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
            {
                throw new InvalidOperationException("Parameters not supported");
            }

            string code = !_templateTypeMapping.ContainsKey(schemeCode.ToLower()) ? schemeCode : _templateTypeMapping[schemeCode.ToLower()];

            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);

            WorkflowScheme scheme = await (await dbcoll.FindAsync(c => c.Code == code).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);


            if (scheme == null)
            {
                throw new InvalidOperationException($"Scheme with Code={code} not found");
            }

            return XElement.Parse(scheme.Scheme);
        }

        // ReSharper disable once UnusedMember.Global
        public void AddMapping(string processName, object generatorSource)
        {
            string value = generatorSource as string;
            if (value == null)
            {
                throw new InvalidOperationException("Generator source must be a string");
            }

            _templateTypeMapping.Add(processName.ToLower(), value);
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

        private async Task<Tuple<long, WorkflowRuntimeModel>> UpdateWorkflowRuntimeAsync(WorkflowRuntimeModel runtime, Action<WorkflowRuntimeModel> setter,
            UpdateDefinition<Models.WorkflowRuntime> updater)
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            Guid oldLock = runtime.Lock;
            runtime.Lock = Guid.NewGuid();
            setter(runtime);

            UpdateResult result = await dbcoll.UpdateOneAsync(x => x.RuntimeId == runtime.RuntimeId && x.Lock == oldLock,
                updater.Set(x => x.Lock, runtime.Lock)
            ).ConfigureAwait(false);

            if (result.MatchedCount != 1)
            {
                return new Tuple<long, WorkflowRuntimeModel>(result.ModifiedCount, null);
            }

            return new Tuple<long, WorkflowRuntimeModel>(result.ModifiedCount, runtime);
        }

        #region IApprovalProvider

        public  async Task DropWorkflowInboxAsync(Guid processId)
        {
            IMongoCollection<WorkflowInbox> dCollection = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            
            await dCollection.DeleteManyAsync(c => c.ProcessId == processId)
                .ConfigureAwait(false);
        }

        public  async Task InsertInboxAsync(List<InboxItem> newActors)
        {
            IMongoCollection<WorkflowInbox> dCollection = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            WorkflowInbox[] inboxItems = newActors.Select(WorkflowInbox.ToDB).ToArray();
            if (inboxItems.Any())
            {
                await dCollection.InsertManyAsync(inboxItems).ConfigureAwait(false);
            }
        }

        public async Task<int> GetInboxCountByProcessIdAsync(Guid processId)
        {
            IMongoCollection<WorkflowInbox> dCollection = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            return (int)await dCollection.CountDocumentsAsync(x => x.ProcessId == processId).ConfigureAwait(false);
        }

        public async Task<int> GetInboxCountByIdentityIdAsync(string identityId)
        {
            IMongoCollection<WorkflowInbox> dCollection = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            return (int)await dCollection.CountDocumentsAsync(x => x.IdentityId == identityId).ConfigureAwait(false);
        }

        public async Task<List<InboxItem>> GetInboxByProcessIdAsync(Guid processId, Paging paging = null, CultureInfo culture = null)
        {
            IMongoCollection<WorkflowInbox> dCollection = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            IMongoQueryable<WorkflowInbox> query = dCollection.AsQueryable()
                .Where(x => x.ProcessId == processId)
                .OrderByDescending(x => x.AddingDate);
            
            if (paging != null)
            {
                query = query.Skip(paging.SkipCount())
                    .Take(paging.PageSize);
            }
            
            List<WorkflowInbox> inboxItems = await query.ToListAsync().ConfigureAwait(false);
            
            return await WorkflowInbox.FromDB(_runtime, inboxItems.ToArray(), culture ?? CultureInfo.CurrentCulture)
                .ConfigureAwait(false);
        }

        public async Task<List<InboxItem>> GetInboxByIdentityIdAsync(string identityId, Paging paging = null, CultureInfo culture = null)
        {
            IMongoCollection<WorkflowInbox> dCollection = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);

            IMongoQueryable<WorkflowInbox> query = dCollection.AsQueryable()
                .Where(x => x.IdentityId == identityId)
                .OrderByDescending(x => x.AddingDate);
            
            if (paging != null)
            {
                query = query.Skip(paging.SkipCount())
                    .Take(paging.PageSize);
            }

            List<WorkflowInbox> inboxItems = await query.ToListAsync().ConfigureAwait(false);
            
            return await WorkflowInbox.FromDB(_runtime, inboxItems.ToArray(), culture ?? CultureInfo.CurrentCulture)
                .ConfigureAwait(false);
        }

        public async Task FillApprovalHistoryAsync(ApprovalHistoryItem approvalHistoryItem)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            WorkflowApprovalHistory historyItem = await (await dCollection.FindAsync(h => h.ProcessId == approvalHistoryItem.ProcessId 
                        && !h.TransitionTime.HasValue 
                        && h.InitialState == approvalHistoryItem.InitialState
                        && h.DestinationState == approvalHistoryItem.DestinationState)
                        .ConfigureAwait(false))
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(false);

            if (historyItem == null)
            {
                historyItem = WorkflowApprovalHistory.ToDB(approvalHistoryItem);
                
                await dCollection.InsertOneAsync(historyItem).ConfigureAwait(false);
            }
            else
            {
                await dCollection.UpdateOneAsync(x => x.Id == historyItem.Id,
                    Builders<WorkflowApprovalHistory>.Update
                        .Set(x => x.TriggerName, approvalHistoryItem.TriggerName)
                        .Set(x => x.TransitionTime, _runtime.RuntimeDateTimeNow)
                        .Set(x => x.IdentityId, approvalHistoryItem.IdentityId)
                        .Set(x => x.Commentary, approvalHistoryItem.Commentary))
                        .ConfigureAwait(false);
            }
        }

        public virtual async Task DropEmptyApprovalHistoryAsync(Guid processId)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            await dCollection.DeleteManyAsync(h => h.ProcessId == processId && !h.TransitionTime.HasValue).ConfigureAwait(false);
        }

        public async Task DropApprovalHistoryByProcessIdAsync(Guid processId)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            await dCollection.DeleteManyAsync(h => h.ProcessId == processId).ConfigureAwait(false);
        }

        public async Task DropApprovalHistoryByIdentityIdAsync(string identityId)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            await dCollection.DeleteManyAsync(h => h.IdentityId == identityId).ConfigureAwait(false);
        }

        public async Task<int> GetApprovalHistoryCountByProcessIdAsync(Guid processId)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            return (int)await dCollection.CountDocumentsAsync(x => x.ProcessId == processId).ConfigureAwait(false);
        }

        public async Task<int> GetApprovalHistoryCountByIdentityIdAsync(string identityId)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            return (int)await dCollection.CountDocumentsAsync(x => x.IdentityId == identityId).ConfigureAwait(false);
        }

        public async Task<List<ApprovalHistoryItem>> GetApprovalHistoryByProcessIdAsync(Guid processId, Paging paging = null)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            
            IMongoQueryable<WorkflowApprovalHistory> query = dCollection.AsQueryable()
                .Where(x => x.ProcessId == processId)
                .OrderBy(x => x.Sort);
            
            if (paging != null)
            {
                query = query.Skip(paging.SkipCount())
                    .Take(paging.PageSize);
            }
            
            List<WorkflowApprovalHistory> approvalHistory = await query.ToListAsync().ConfigureAwait(false);

            return approvalHistory.Select(x=>WorkflowApprovalHistory.FromDB(_runtime, x)).ToList();
        }

        public async Task<List<ApprovalHistoryItem>> GetApprovalHistoryByIdentityIdAsync(string identityId, Paging paging = null)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);

            IMongoQueryable<WorkflowApprovalHistory> query = dCollection.AsQueryable()
                .Where(x => x.IdentityId == identityId)
                .OrderBy(x => x.Sort);
            
            if (paging != null)
            {
                query = query.Skip(paging.SkipCount())
                    .Take(paging.PageSize);
            }
            
            List<WorkflowApprovalHistory> approvalHistory = await query.ToListAsync().ConfigureAwait(false);
            
            return approvalHistory.Select(x=>WorkflowApprovalHistory.FromDB(_runtime, x)).ToList();
        }

        public async Task<int> GetOutboxCountByIdentityIdAsync(string identityId)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            
            return dCollection.Aggregate().Match(x=>x.IdentityId == identityId).Group(
                x => x.ProcessId,y=>new
                {
                    id = y.Key
                }).ToList().Count();
        }

        public async Task<List<OutboxItem>> GetOutboxByIdentityIdAsync(string identityId, Paging paging = null)
        {
            IMongoCollection<WorkflowApprovalHistory> dCollection = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            List<OutboxItem> outboxItems;

            if (paging == null)
            {
                 outboxItems = await dCollection.Aggregate().Match(x=>x.IdentityId == identityId)
                     .Group(
                    x => x.ProcessId
                    , g =>
                        new OutboxItem()
                        {
                            ProcessId = g.Key,
                            FirstApprovalTime = g.Min(x => x.TransitionTime),
                            LastApprovalTime = g.Max(x => x.TransitionTime),
                            ApprovalCount = g.Count()
                        }).SortByDescending(x => x.LastApprovalTime)
                     .ToListAsync()
                     .ConfigureAwait(false);
            }
            else
            {
                outboxItems = await dCollection.Aggregate().Match(x=>x.IdentityId == identityId)
                    .Group(
                        x => x.ProcessId
                        , g =>
                            new OutboxItem()
                            {
                                ProcessId = g.Key,
                                FirstApprovalTime = g.Min(x => x.TransitionTime),
                                LastApprovalTime = g.Max(x => x.TransitionTime),
                                ApprovalCount = g.Count(),
                            }).SortByDescending(x => x.LastApprovalTime)
                    .Skip(paging.SkipCount())
                    .Limit(paging.PageSize)
                    .ToListAsync().ConfigureAwait(false);
            }
            IEnumerable<Guid> processIds = outboxItems.Select(x => x.ProcessId).Distinct();


            var history = new Dictionary<Guid, string>();
            
            foreach (OutboxItem item in outboxItems)
            {
                WorkflowApprovalHistory historyItem = (await dCollection
                    .Find(x => x.ProcessId == item.ProcessId)
                    .SortByDescending(x => x.TransitionTime)
                    .Limit(1)
                    .ToListAsync()
                    .ConfigureAwait(false)).FirstOrDefault();

                if (historyItem!=null)
                {
                    history.Add(historyItem.ProcessId, historyItem.TriggerName);
                }
            }

            foreach (OutboxItem outbox in outboxItems)
            {
                if (history.TryGetValue(outbox.ProcessId, out string command))
                {
                    outbox.LastApproval = command;
                }

                outbox.FirstApprovalTime = _runtime.ToRuntimeTime(outbox.FirstApprovalTime);
                outbox.LastApprovalTime = _runtime.ToRuntimeTime(outbox.LastApprovalTime);
            }

            return outboxItems;
        }
        
        
        #endregion IApprovalProvider

        private void CheckInitialData()
        {
            IMongoCollection<Models.WorkflowSync> lockColl = 
                Store.GetCollection<Models.WorkflowSync>(MongoDBConstants.WorkflowSyncCollectionName);

            lockColl.UpdateOne(x => x.Name == "Timer", Builders<Models.WorkflowSync>.Update
                .SetOnInsert(x => x.Name, "Timer")
                .SetOnInsert(x => x.Lock, Guid.NewGuid()), new UpdateOptions { IsUpsert = true });

            lockColl.UpdateOne(x => x.Name == "ServiceTimer", Builders<Models.WorkflowSync>.Update
              .SetOnInsert(x => x.Name, "ServiceTimer")
              .SetOnInsert(x => x.Lock, Guid.NewGuid()), new UpdateOptions { IsUpsert = true });
        }

        private static string GetOrderParameters(List<(string parameterName,SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"{x.parameterName} {x.sortDirection.UpperName()}"));
            return result;
        }
    }
}
