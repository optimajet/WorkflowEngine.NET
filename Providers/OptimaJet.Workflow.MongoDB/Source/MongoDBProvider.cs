using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Runtime.Timers;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.MongoDB.Models;
using OptimaJet.Workflow.Plugins;
using SortDirection = OptimaJet.Workflow.Core.Persistence.SortDirection;
using WorkflowForm = OptimaJet.Workflow.Core.Persistence.WorkflowForm;
using WorkflowRuntime = OptimaJet.Workflow.Core.Runtime.WorkflowRuntime;

namespace OptimaJet.Workflow.MongoDB
{
    public static class MongoDBConstants
    {
        public const string WorkflowProcessInstanceCollectionName = "WorkflowProcessInstance";
        public const string WorkflowProcessSchemeCollectionName = "WorkflowProcessScheme";
        public const string WorkflowProcessTransitionHistoryCollectionName = "WorkflowProcessTransitionHistory";
        public const string WorkflowSchemeCollectionName = "WorkflowScheme";
        public const string WorkflowProcessTimerCollectionName = "WorkflowProcessTimer";
        [Obsolete("Do not use Assignment Plugin or related API. It will be removed soon.")]
        public const string WorkflowProcessAssignmentCollectionName = "WorkflowProcessAssignment";
        public const string WorkflowGlobalParameterCollectionName = "WorkflowGlobalParameter";
        public const string WorkflowRuntimeCollectionName = "WorkflowRuntime";
        public const string WorkflowSyncCollectionName = "WorkflowSync";
        public const string WorkflowFormCollectionName = "WorkflowForm";
        public const string WorkflowApprovalHistoryCollectionName = "WorkflowApprovalHistory";
        public const string WorkflowInboxCollectionName = "WorkflowInbox";
    }

    public class MongoDBProvider : IWorkflowProvider
    {
        private const int CreateNewFormVersionMaxAttempts = 3;
        private const int CreateNewFormVersionRetryDelayMilliseconds = 50;

        private WorkflowRuntime _runtime;
        private readonly bool _writeToHistory;
        private readonly bool _writeSubProcessToRoot;

        public MongoDBProvider(string connectionString, bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            string databaseName = MongoUrl.Create(connectionString).DatabaseName;
            var client = new MongoClient(connectionString);
            Store = client.GetDatabase(databaseName);

            ConnectionString = connectionString;
            _writeToHistory = writeToHistory;
            _writeSubProcessToRoot = writeSubProcessToRoot;
        }

        public string Id => PersistenceProviderId.Mongo;

        public string ConnectionString { get; }

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
                XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete,
                workflowProcessScheme.AllowedActivities, workflowProcessScheme.StartingTransition, workflowProcessScheme.TenantId);
        }

        private static FilterDefinition<WorkflowProcessScheme> GetWorkflowProcessSchemeTenantFilter(string tenantId)
        {
            return tenantId == null
                ? Builders<WorkflowProcessScheme>.Filter.Eq(scheme => scheme.TenantId, null)
                : Builders<WorkflowProcessScheme>.Filter.Eq(scheme => scheme.TenantId, tenantId);
        }

        private static FilterDefinition<WorkflowScheme> GetWorkflowSchemeExactFilter(string schemeCode, string tenantId)
        {
            var filterBuilder = Builders<WorkflowScheme>.Filter;
            FilterDefinition<WorkflowScheme> tenantFilter = tenantId == null
                ? filterBuilder.Eq(scheme => scheme.TenantId, null)
                : filterBuilder.Eq(scheme => scheme.TenantId, tenantId);

            return filterBuilder.And(
                filterBuilder.Eq(scheme => scheme.Code, schemeCode),
                tenantFilter);
        }

        private async Task<WorkflowScheme> GetWorkflowSchemeAsync(IMongoCollection<WorkflowScheme> collection,
            string schemeCode, string tenantId)
        {
            WorkflowScheme scheme = await (await collection.FindAsync(GetWorkflowSchemeExactFilter(schemeCode, tenantId))
                    .ConfigureAwait(false))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (scheme != null || tenantId == null)
            {
                return scheme;
            }

            return await (await collection.FindAsync(GetWorkflowSchemeExactFilter(schemeCode, null)).ConfigureAwait(false))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
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
                return (await (await dbcoll.FindAsync(filter,options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(x => x.GetValue("_id").AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy)).ToList();
            }

            var filters = new List<FilterDefinition<WorkflowProcessInstance>> {filter, Builders<WorkflowProcessInstance>.Filter.Eq(n => n.Status.RuntimeId, runtimeId)};

            FilterDefinition<WorkflowProcessInstance> combinedFilter = Builders<WorkflowProcessInstance>.Filter.And(filters);
            
            return (await (await dbcoll.FindAsync(combinedFilter,options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(x => x.GetValue("_id").AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy)).ToList();

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

        public Task DropUnusedWorkflowProcessSchemeAsync()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<List<ProcessInstanceItem>> GetProcessInstancesAsync(
            List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
        {
            IMongoCollection<WorkflowProcessInstance> workflowProcessInstanceCollection =
                Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

            var sortDefinitions = new List<SortDefinition<WorkflowProcessInstance>>();
            if (orderParameters != null && orderParameters.Count > 0)
            {
                foreach ((string parameterName, SortDirection sortDirection) in orderParameters)
                {
                    SortDefinition<WorkflowProcessInstance> sort = sortDirection == SortDirection.Desc
                        ? Builders<WorkflowProcessInstance>.Sort.Descending(parameterName)
                        : Builders<WorkflowProcessInstance>.Sort.Ascending(parameterName);
                    sortDefinitions.Add(sort);
                }
            }

            if (sortDefinitions.Count == 0)
            {
                sortDefinitions.Add(Builders<WorkflowProcessInstance>.Sort.Ascending("Id"));
            }

            SortDefinition<WorkflowProcessInstance> combinedSort =
                Builders<WorkflowProcessInstance>.Sort.Combine(sortDefinitions);

            List<WorkflowProcessInstance> processInstances = await workflowProcessInstanceCollection
                .Find(FilterDefinition<WorkflowProcessInstance>.Empty)
                .Sort(combinedSort)
                .Skip(paging?.SkipCount() ?? 0)
                .Limit(paging?.PageSize ?? 0)
                .ToListAsync().ConfigureAwait(false);

            var schemeIds = processInstances.Select(x => x.SchemeId).ToList();
            IMongoCollection<WorkflowProcessScheme> workflowProcessSchemeCollection =
                Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            FilterDefinition<WorkflowProcessScheme> workflowProcessSchemeFilter =
                Builders<WorkflowProcessScheme>.Filter.In(processScheme => processScheme.Id, schemeIds);
            ProjectionDefinition<WorkflowProcessScheme> projection = Builders<WorkflowProcessScheme>.Projection
                .Include(processScheme => processScheme.Id)
                .Include(processScheme => processScheme.StartingTransition);

            List<BsonDocument> schemes = await workflowProcessSchemeCollection
                .Find(workflowProcessSchemeFilter)
                .Project(projection)
                .ToListAsync().ConfigureAwait(false);

            return processInstances.Join(
                schemes,
                processInstance => processInstance.SchemeId,
                scheme => scheme["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy),
                (processInstance, scheme) => new ProcessInstanceItem
                {
                    ActivityName = processInstance.ActivityName,
                    Id = processInstance.Id,
                    PreviousActivity = processInstance.PreviousActivity,
                    PreviousActivityForDirect = processInstance.PreviousActivityForDirect,
                    PreviousActivityForReverse = processInstance.PreviousActivityForReverse,
                    PreviousState = processInstance.PreviousState,
                    PreviousStateForDirect = processInstance.PreviousStateForDirect,
                    PreviousStateForReverse = processInstance.PreviousStateForReverse,
                    SchemeId = processInstance.SchemeId,
                    StateName = processInstance.StateName,
                    ParentProcessId = processInstance.ParentProcessId,
                    RootProcessId = processInstance.RootProcessId,
                    TenantId = processInstance.TenantId,
                    SubprocessName = processInstance.SubprocessName,
                    CreationDate = processInstance.CreationDate,
                    LastTransitionDate = processInstance.LastTransitionDate,
                    StartingTransition =
                        scheme[nameof(WorkflowProcessScheme.StartingTransition)] == BsonNull.Value
                            ? null
                            : scheme[nameof(WorkflowProcessScheme.StartingTransition)].AsString,
                    CalendarName = processInstance.CalendarName
                }).ToList();
        }

        public virtual async Task<int> GetProcessInstancesCountAsync()
        {
            IMongoCollection<WorkflowProcessInstance> workflowProcessInstanceCollection =
                Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            long count = await workflowProcessInstanceCollection.CountDocumentsAsync(_ => true).ConfigureAwait(false);
            return Convert.ToInt32(count);
        }

        public virtual async Task<List<SchemeItem>> GetSchemesAsync(
            List<(string parameterName, SortDirection sortDirection)> orderParameters = null, Paging paging = null)
        {
            IMongoCollection<WorkflowScheme> workflowSchemeCollection =
                Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);

            var sortDefinitions = new List<SortDefinition<WorkflowScheme>>();
            if (orderParameters != null && orderParameters.Count > 0)
            {
                foreach ((string parameterName, SortDirection sortDirection) in orderParameters)
                {
                    SortDefinition<WorkflowScheme> sortDefinition = sortDirection == SortDirection.Desc
                        ? Builders<WorkflowScheme>.Sort.Descending(parameterName)
                        : Builders<WorkflowScheme>.Sort.Ascending(parameterName);

                    sortDefinitions.Add(sortDefinition);
                }
            }

            if (paging != null && sortDefinitions.Count == 0)
            {
                // Other providers use WorkflowScheme.Code as the default sort for paging.
                sortDefinitions.Add(Builders<WorkflowScheme>.Sort.Ascending(nameof(WorkflowScheme.Code)));
            }

            FilterDefinition<WorkflowScheme> filter = Builders<WorkflowScheme>.Filter.Empty;
            IFindFluent<WorkflowScheme, WorkflowScheme> query = workflowSchemeCollection.Find(filter);

            if (sortDefinitions.Count > 0)
            {
                SortDefinition<WorkflowScheme> combinedSort = Builders<WorkflowScheme>.Sort.Combine(sortDefinitions);
                query = query.Sort(combinedSort);
            }

            if (paging != null)
            {
                query = query
                    .Skip(paging.SkipCount())
                    .Limit(paging.PageSize);
            }

            List<WorkflowScheme> schemes = await query.ToListAsync().ConfigureAwait(false);
            
            return schemes.Select(scheme => new SchemeItem
            {
                Code = scheme.Code,
                Scheme = scheme.Scheme,
                CanBeInlined = scheme.CanBeInlined,
                InlinedSchemes = scheme.InlinedSchemes,
                Tags = scheme.Tags
            }).ToList();
        }

        public virtual async Task<int> GetSchemesCountAsync()
        {
            IMongoCollection<WorkflowScheme> dbcoll =
                Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            long count = await dbcoll.CountDocumentsAsync(_ => true).ConfigureAwait(false);
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

            IMongoCollection<WorkflowProcessScheme> processSchemeCollection =
                Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            UpdateDefinition<WorkflowProcessScheme> updateDefinition = Builders<WorkflowProcessScheme>.Update
                .Set(processScheme => processScheme.StartingTransition, processInstance.ProcessScheme.StartingTransition);
            await processSchemeCollection.UpdateOneAsync(processScheme => processScheme.Id == processInstance.SchemeId,
                updateDefinition);
            
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
                CreationDate = processInstance.CreationDate,
                CalendarName = processInstance.CalendarName
            };
            await dbcoll.InsertOneAsync(newProcess).ConfigureAwait(false);
        }

        public virtual async Task BindProcessToNewSchemeAsync(ProcessInstance processInstance)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance oldProcess = await (await dbcoll.FindAsync(x => x.Id == processInstance.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (oldProcess == null)
            {
                throw new ProcessNotFoundException(processInstance.ProcessId);
            }

            oldProcess.SchemeId = processInstance.SchemeId;

            await SaveAsync(dbcoll, oldProcess, doc => doc.Id == oldProcess.Id).ConfigureAwait(false);
        }

        private async Task SaveAsync<T>(IMongoCollection<T> collection, T obj, Expression<Func<T, bool>> filter)
        {
            await collection.ReplaceOneAsync<T>(filter, obj, new ReplaceOptions() { IsUpsert = true }).ConfigureAwait(false);
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

        private async Task SaveParameterBatchAsync(PersistenceParametersBatch batch)
        {
            if (batch.IsEmpty)
            {
                return;
            }

            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance process = await (await dbcoll.FindAsync(x => x.Id == batch.ProcessId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            
            if (process != null)
            {
                if (process.Persistence == null)
                {
                    process.Persistence = new List<WorkflowProcessInstancePersistence>();
                }

                foreach (SerializedParameter serializedParameter in batch.Parameters)
                {
                    WorkflowProcessInstancePersistence persistence = process.Persistence.SingleOrDefault(pp => pp.ParameterName == serializedParameter.Name);

                    if (serializedParameter.Operation == SerializedParameter.PersistenceOperation.Delete)
                    {
                        // Delete parameter
                        if (persistence != null)
                        {
                            process.Persistence.Remove(persistence);
                        }
                    }
                    else
                    {
                        // Insert or update parameter
                        if (persistence == null)
                        {
                            persistence = new WorkflowProcessInstancePersistence
                            {
                                ParameterName = serializedParameter.Name,
                                Value = serializedParameter.SerializedValue
                            };
                            process.Persistence.Add(persistence);
                        }
                        else
                        {
                            persistence.Value = serializedParameter.SerializedValue;
                        }
                    }
                }

                await SaveAsync(dbcoll, process, doc => doc.Id == process.Id).ConfigureAwait(false);
                batch.CommitFlags();
            }
        }

        public virtual async Task SavePersistenceParametersAsync(ProcessInstance processInstance)
        {
            var batch = PersistenceParametersBatch.CreateFromProcessInstance(processInstance);
            await SaveParameterBatchAsync(batch).ConfigureAwait(false);
        }
        
        public virtual async Task SavePersistenceParameterAsync(ProcessInstance processInstance, string parameterName)
        {
            var batch = PersistenceParametersBatch.CreateForSingleParameter(processInstance, parameterName);
            if (batch.IsEmpty)
            {
                throw new InvalidOperationException("Cannot save parameter for empty batch");
            }
            await SaveParameterBatchAsync(batch).ConfigureAwait(false);
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
                TenantId = processInstance.TenantId,
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

                await SaveAsync(dbcoll, inst, doc => doc.Id == inst.Id).ConfigureAwait(false);
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
                ActorName= actorName,
                ExecutorName = executorName,
                Id = Guid.NewGuid(),
                IsFinalised = transition.To.IsFinal,
                ProcessId = _writeSubProcessToRoot && processInstance.IsSubprocess ? processInstance.RootProcessId : processInstance.ProcessId,
                TenantId = processInstance.TenantId,
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

        public virtual async Task<bool> IsProcessExistsAsync(Guid processId, string tenantId)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            FilterDefinitionBuilder<WorkflowProcessInstance> filterBuilder = Builders<WorkflowProcessInstance>.Filter;

            if (tenantId == null)
            {
                FilterDefinition<WorkflowProcessInstance> filter =
                    filterBuilder.Eq(x => x.Id, processId) &
                    filterBuilder.Eq(x => x.TenantId, null);

                return await dbcoll.CountDocumentsAsync(filter)
                    .ConfigureAwait(false) != 0;
            }

            FilterDefinition<WorkflowProcessInstance> sameTenantFilter =
                filterBuilder.Eq(x => x.Id, processId) &
                filterBuilder.Eq(x => x.TenantId, tenantId);

            return await dbcoll.CountDocumentsAsync(sameTenantFilter)
                .ConfigureAwait(false) != 0;
        }
        
        public virtual async Task<ProcessStatus> GetInstanceStatusAsync(Guid processId)
        {
            IMongoCollection<WorkflowProcessInstance> dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance instance = await (await dbcoll.FindAsync(x => x.Id == processId).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
            WorkflowProcessInstanceStatus instanceStatus = instance?.Status;
            
            if (instanceStatus == null)
            {
                return ProcessStatus.NotFound;
            }

            ProcessStatus status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instanceStatus.Status);
            
            return status ?? ProcessStatus.Unknown;
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
                     _runtime.ToRuntimeTime(processInstance.LastTransitionDate)),
                ParameterDefinition.Create(
                    systemParameters.Single(sp => sp.Name == DefaultDefinitions.ParameterCalendarName.Name),
                    processInstance.CalendarName)
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

            ParameterDefinitionWithValue result;
            if (parameterDefinition == null)
            {
                parameterDefinition = ParameterDefinition.Create(persistedParameter.ParameterName, typeof(UnknownParameterType), ParameterPurpose.Persistence);
                result = ParameterDefinition.CreateFromPersistence(parameterDefinition, persistedParameter.Value);
            }
            else
            {
                result = ParameterDefinition.CreateFromPersistence(parameterDefinition, ParametersSerializer.Deserialize(persistedParameter.Value, parameterDefinition.Type));
            }

            return result;
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

        public virtual Task SaveGlobalParameterAsync<T>(string type, string name, T value)
        {
            return SaveGlobalParameterAsync(new TenantGlobalParameterKey { Type = type, TenantId = null, Name = name }, value);
        }

        public virtual Task SaveTenantGlobalParameterAsync<T>(TenantGlobalParameterKey key, T value)
        {
            return SaveGlobalParameterAsync(key, value);
        }

        private async Task SaveGlobalParameterAsync<T>(TenantGlobalParameterKey key, T value)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            WorkflowGlobalParameter parameter = await (await dbcoll.FindAsync(GetGlobalParameterPredicate(key)).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (parameter == null)
            {
                parameter = new WorkflowGlobalParameter
                {
                    Id = Guid.NewGuid(),
                    Name = key.Name,
                    Type = key.Type,
                    TenantId = key.TenantId,
                    Value = Newtonsoft.Json.JsonConvert.SerializeObject(value)
                };

                await dbcoll.InsertOneAsync(parameter).ConfigureAwait(false);
            }
            else
            {
                parameter.Value = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                await SaveAsync(dbcoll, parameter, doc => doc.Id == parameter.Id).ConfigureAwait(false);
            }
        }

        public virtual Task<T> LoadGlobalParameterAsync<T>(string type, string name)
        {
            return LoadGlobalParameterAsync<T>(new TenantGlobalParameterKey { Type = type, TenantId = null, Name = name });
        }

        public virtual Task<T> LoadTenantGlobalParameterAsync<T>(TenantGlobalParameterKey key)
        {
            return LoadGlobalParameterAsync<T>(key);
        }

        private async Task<T> LoadGlobalParameterAsync<T>(TenantGlobalParameterKey key)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            WorkflowGlobalParameter parameter = await (await dbcoll.FindAsync(GetGlobalParameterPredicate(key)).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

            if (parameter != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(parameter.Value);
            }

            return default;
        }

        public Task<Dictionary<string, T>> LoadGlobalParametersWithNamesAsync<T>(string type, Sorting sort = null)
        {
            return LoadGlobalParametersWithNamesAsync<T>(new TenantGlobalParameterScope { Type = type, TenantId = null }, sort);
        }

        public Task<Dictionary<string, T>> LoadTenantGlobalParametersWithNamesAsync<T>(TenantGlobalParameterScope scope, Sorting sort = null)
        {
            return LoadGlobalParametersWithNamesAsync<T>(scope, sort);
        }

        private async Task<Dictionary<string, T>> LoadGlobalParametersWithNamesAsync<T>(TenantGlobalParameterScope scope, Sorting sort = null)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            var findOptions = sort is null ? null : new FindOptions<WorkflowGlobalParameter, WorkflowGlobalParameter>
            {
                Sort = GetSortDefinition<WorkflowGlobalParameter>(sort)
            };

            var asyncCursor = await dbcoll.FindAsync(GetGlobalParameterPredicate(scope), findOptions).ConfigureAwait(false);
            var parameters = await asyncCursor.ToListAsync().ConfigureAwait(false);

            var dict = new Dictionary<string, T>();
            foreach (var parameter in parameters)
            {
                dict[parameter.Name] = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(parameter.Value);
            }
            
            return dict;
        }
        
        public virtual Task<List<T>> LoadGlobalParametersAsync<T>(string type, Sorting sort = null)
        {
            return LoadGlobalParametersAsync<T>(new TenantGlobalParameterScope { Type = type, TenantId = null }, sort);
        }

        public virtual Task<List<T>> LoadTenantGlobalParametersAsync<T>(TenantGlobalParameterScope scope, Sorting sort = null)
        {
            return LoadGlobalParametersAsync<T>(scope, sort);
        }

        private async Task<List<T>> LoadGlobalParametersAsync<T>(TenantGlobalParameterScope scope, Sorting sort = null)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            var findOptions = sort is null ? null : new FindOptions<WorkflowGlobalParameter, WorkflowGlobalParameter>
            {
                Sort = GetSortDefinition<WorkflowGlobalParameter>(sort)
            };
            
            var findAsync = dbcoll.FindAsync(GetGlobalParameterPredicate(scope), findOptions);
            var asyncCursor = await findAsync.ConfigureAwait(false);
            var parameters = await asyncCursor.ToListAsync().ConfigureAwait(false);
            
            return parameters.Select(gp => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(gp.Value)).ToList();
        }
        
        public virtual Task<PagedResponse<T>> LoadGlobalParametersWithPagingAsync<T>(string type, Paging paging, string name = null,
            Sorting sort = null)
        {
            return LoadGlobalParametersWithPagingAsync<T>(new TenantGlobalParameterScope { Type = type, TenantId = null }, paging, name, sort);
        }

        public virtual Task<PagedResponse<T>> LoadTenantGlobalParametersWithPagingAsync<T>(TenantGlobalParameterScope scope, Paging paging,
            Sorting sort = null)
        {
            return LoadGlobalParametersWithPagingAsync<T>(scope, paging, null, sort);
        }

        private async Task<PagedResponse<T>> LoadGlobalParametersWithPagingAsync<T>(TenantGlobalParameterScope scope, Paging paging,
            string name = null, Sorting sort = null)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll =
                Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);
            var parametersQuery = dbcoll.Aggregate().Match(GetGlobalParameterPredicate(new TenantGlobalParameterScope { Type = scope.Type, TenantId = scope.TenantId }));
            var countQuery = dbcoll.AsQueryable().Where(GetGlobalParameterPredicate(new TenantGlobalParameterScope { Type = scope.Type, TenantId = scope.TenantId }));

            if (!String.IsNullOrEmpty(name))
            {
                parametersQuery = parametersQuery.Match(c => c.Name.ToLower().Contains(name.ToLower()));
                countQuery = countQuery.Where(c => c.Name.ToLower().Contains(name.ToLower()));
            }

            sort ??= Sorting.Create(nameof(WorkflowGlobalParameter.Name));
            parametersQuery = parametersQuery.Sort(GetSortDefinition<WorkflowGlobalParameter>(sort));

            var count = await countQuery.CountAsync().ConfigureAwait(false);
            var parameters = parametersQuery.Skip(paging.SkipCount()).Limit(paging.PageSize).ToList();

            return new PagedResponse<T>()
            {
                Data = parameters.Select(c => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(c.Value)).ToList(),
                Count = count
            };
        }

        public virtual Task DeleteGlobalParametersAsync(string type, string name = null)
        {
            return name == null
                ? DeleteGlobalParametersAsync(new TenantGlobalParameterScope { Type = type, TenantId = null })
                : DeleteGlobalParameterAsync(new TenantGlobalParameterKey { Type = type, TenantId = null, Name = name });
        }

        public virtual Task DeleteTenantGlobalParametersAsync(TenantGlobalParameterScope scope)
        {
            return DeleteGlobalParametersAsync(scope);
        }

        public virtual Task DeleteTenantGlobalParameterAsync(TenantGlobalParameterKey key)
        {
            return DeleteGlobalParameterAsync(key);
        }

        private async Task DeleteGlobalParametersAsync(TenantGlobalParameterScope scope)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            await dbcoll.DeleteManyAsync(GetGlobalParameterPredicate(scope)).ConfigureAwait(false);
        }

        private async Task DeleteGlobalParameterAsync(TenantGlobalParameterKey key)
        {
            IMongoCollection<WorkflowGlobalParameter> dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            await dbcoll.DeleteManyAsync(GetGlobalParameterPredicate(key)).ConfigureAwait(false);
        }

        private static Expression<Func<WorkflowGlobalParameter, bool>> GetGlobalParameterPredicate(TenantGlobalParameterScope scope)
        {
            string type = scope.Type;
            string tenantId = scope.TenantId;

            return tenantId == null
                ? item => item.Type == type && item.TenantId == null
                : item => item.Type == type && item.TenantId == tenantId;
        }

        private static Expression<Func<WorkflowGlobalParameter, bool>> GetGlobalParameterPredicate(TenantGlobalParameterKey key)
        {
            string type = key.Type;
            string name = key.Name;
            string tenantId = key.TenantId;

            return tenantId == null
                ? item => item.Type == type && item.Name == name && item.TenantId == null
                : item => item.Type == type && item.Name == name && item.TenantId == tenantId;
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

        public virtual async Task RegisterTimerAsync(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, string tenantId, bool notOverrideIfExists)
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
                    Ignore = false,
                    TenantId = tenantId
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
            if (top <= 0)
            {
                throw new ArgumentException(ArgumentExceptionMessages.ArgumentMustBePositive(nameof(top), top));
            }
            
            DateTime currentTime = _runtime.RuntimeDateTimeNow.ToUniversalTime();

            IMongoCollection<WorkflowProcessTimer> timerCollection =
                Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);

            List<WorkflowProcessTimer> workflowTimers = await timerCollection
                .Find(timer => !timer.Ignore && timer.NextExecutionDateTime <= currentTime)
                .Sort(Builders<WorkflowProcessTimer>.Sort.Ascending(timer => timer.NextExecutionDateTime))
                .Limit(top)
                .ToListAsync()
                .ConfigureAwait(false);

            return workflowTimers.Select(timer => new Core.Model.WorkflowTimer
            {
                Name = timer.Name,
                ProcessId = timer.ProcessId,
                TimerId = timer.Id,
                NextExecutionDateTime = _runtime.ToRuntimeTime(timer.NextExecutionDateTime),
                RootProcessId = timer.RootProcessId
            }).ToList();
        }

        public virtual async Task<List<ProcessHistoryItem>> GetProcessHistoryAsync(Guid processId, Paging paging = null)
        {
            IMongoCollection<WorkflowProcessTransitionHistory> dbcoll = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            
            IQueryable<WorkflowProcessTransitionHistory> query = dbcoll.AsQueryable()
                .Where(x => x.ProcessId == processId)
                .OrderBy(x => x.TransitionTime);
            
            if (paging != null)
            {
                query = query.Skip(paging.SkipCount())
                    .Take(paging.PageSize);
            }
            
            List<WorkflowProcessTransitionHistory> history = await query.ToListAsync().ConfigureAwait(false);
            
            return history.Select(hi => new ProcessHistoryItem
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
                    TransitionClassifier = (TransitionClassifier)Enum.Parse(typeof(TransitionClassifier), hi.TransitionClassifier),
                    TransitionTime = _runtime.ToRuntimeTime(hi.TransitionTime),
                    TriggerName = hi.TriggerName,
                    StartTransitionTime = hi.StartTransitionTime,
                    TransitionDuration = hi.TransitionDuration
                })
                .ToList();
        }

       public Task<int> GetProcessHistoryCountAsync(Guid processId)
       {
           IMongoCollection<WorkflowProcessTransitionHistory> dbcoll = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
           return dbcoll.AsQueryable().Where(c => c.ProcessId == processId).CountAsync();
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
            var schemeIds = instances.Select(i => i[nameof(WorkflowProcessInstance.SchemeId)].AsBsonBinaryData.ToGuid(GuidRepresentation.CSharpLegacy)).Distinct().ToList();

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

            string getterField;
            Func<Models.WorkflowRuntime, DateTime?> getterFunction;

            switch (timerCategory)
            {
                case TimerCategory.Timer:
                    getterField = "NextTimerTime";
                    getterFunction = x => x.NextTimerTime;
                    break;
                case TimerCategory.ServiceTimer:
                    getterField = "NextServiceTimerTime";
                    getterFunction = x => x.NextServiceTimerTime;
                    break;
                default:
                    throw new Exception($"Unknown sync lock name: {timerCategoryName}");
            }

            Models.WorkflowRuntime max =
                await (await runtimeColl.FindAsync(x => x.RuntimeId != runtimeId && x.Status == RuntimeStatus.Alive,
                        new FindOptions<Models.WorkflowRuntime>
                        {
                            Sort = Builders<Models.WorkflowRuntime>.Sort.Descending(getterField),
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

                await runtimeColl.UpdateOneAsync(x => x.RuntimeId == runtimeId, Builders<Models.WorkflowRuntime>.Update.Set(getterField, result)).ConfigureAwait(false);

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

        public virtual async Task<SchemeDefinition<XElement>> GetProcessSchemeWithParametersAsync(string schemeCode, Guid? rootSchemeId, bool ignoreObsolete, string tenantId = null)
        {
            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            var filters = new List<FilterDefinition<WorkflowProcessScheme>>
            {
                Builders<WorkflowProcessScheme>.Filter.Eq(scheme => scheme.SchemeCode, schemeCode),
                GetWorkflowProcessSchemeTenantFilter(tenantId),
                rootSchemeId.HasValue
                    ? Builders<WorkflowProcessScheme>.Filter.Eq(scheme => scheme.RootSchemeId, rootSchemeId.Value)
                    : Builders<WorkflowProcessScheme>.Filter.Eq(scheme => scheme.RootSchemeId, null)
            };

            if (ignoreObsolete)
            {
                filters.Add(Builders<WorkflowProcessScheme>.Filter.Eq(scheme => scheme.IsObsolete, false));
            }

            WorkflowProcessScheme processScheme = await (await dbcoll.FindAsync(Builders<WorkflowProcessScheme>.Filter.And(filters))
                    .ConfigureAwait(false))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return processScheme != null
                ? ConvertToSchemeDefinition(processScheme)
                : throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);
        }

        [Obsolete("Use SetSchemeIsObsoleteAsync(string schemeCode) instead.")]
        public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode, IDictionary<string, object> parameters)
        {
            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            await dbcoll.UpdateManyAsync(
                item => (item.SchemeCode == schemeCode || item.RootSchemeCode == schemeCode),
                Builders<WorkflowProcessScheme>.Update.Set(c => c.IsObsolete, true)).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeIsObsoleteAsync(string schemeCode, string tenantId = null)
        {
            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            FilterDefinition<WorkflowProcessScheme> schemeFilter = Builders<WorkflowProcessScheme>.Filter.Or(
                Builders<WorkflowProcessScheme>.Filter.Eq(item => item.SchemeCode, schemeCode),
                Builders<WorkflowProcessScheme>.Filter.Eq(item => item.RootSchemeCode, schemeCode));

            FilterDefinition<WorkflowProcessScheme> filter = tenantId == null
                ? schemeFilter
                : Builders<WorkflowProcessScheme>.Filter.And(
                    schemeFilter,
                    GetWorkflowProcessSchemeTenantFilter(tenantId));

            await dbcoll.UpdateManyAsync(filter, Builders<WorkflowProcessScheme>.Update.Set(c => c.IsObsolete, true))
                .ConfigureAwait(false);
        }

        public virtual async Task<SchemeDefinition<XElement>> SaveSchemeAsync(SchemeDefinition<XElement> scheme)
        {
            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            var filters = new List<FilterDefinition<WorkflowProcessScheme>>
            {
                Builders<WorkflowProcessScheme>.Filter.Eq(wps => wps.SchemeCode, scheme.SchemeCode),
                Builders<WorkflowProcessScheme>.Filter.Eq(wps => wps.IsObsolete, scheme.IsObsolete),
                GetWorkflowProcessSchemeTenantFilter(scheme.TenantId),
                scheme.RootSchemeId.HasValue
                    ? Builders<WorkflowProcessScheme>.Filter.Eq(wps => wps.RootSchemeId, scheme.RootSchemeId.Value)
                    : Builders<WorkflowProcessScheme>.Filter.Eq(wps => wps.RootSchemeId, null)
            };

            WorkflowProcessScheme existing = await (await dbcoll.FindAsync(Builders<WorkflowProcessScheme>.Filter.And(filters))
                    .ConfigureAwait(false))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (existing != null)
            {
                return ConvertToSchemeDefinition(existing);
            }

            var newProcessScheme = new WorkflowProcessScheme
            {
                Id = scheme.Id,
                Scheme = scheme.Scheme.ToString(),
                SchemeCode = scheme.SchemeCode,
                RootSchemeCode = scheme.RootSchemeCode,
                RootSchemeId = scheme.RootSchemeId,
                AllowedActivities = scheme.AllowedActivities,
                StartingTransition = scheme.StartingTransition,
                IsObsolete = scheme.IsObsolete,
                TenantId = scheme.TenantId
            };

            await dbcoll.InsertOneAsync(newProcessScheme).ConfigureAwait(false);

            return ConvertToSchemeDefinition(newProcessScheme);
        }

        public virtual async Task UpsertSchemeAsync(SchemeDefinition<XElement> scheme)
        {
            IMongoCollection<WorkflowProcessScheme> dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            var filter = Builders<WorkflowProcessScheme>.Filter
                .Eq(scheme => scheme.Id, scheme.Id);

            var update = Builders<WorkflowProcessScheme>.Update
                .Set(scheme => scheme.Id, scheme.Id)
                .Set(scheme => scheme.Scheme, scheme.Scheme.ToString())
                .Set(scheme => scheme.SchemeCode, scheme.SchemeCode)
                .Set(scheme => scheme.RootSchemeCode, scheme.RootSchemeCode)
                .Set(scheme => scheme.RootSchemeId, scheme.RootSchemeId)
                .Set(scheme => scheme.AllowedActivities, scheme.AllowedActivities)
                .Set(scheme => scheme.StartingTransition, scheme.StartingTransition)
                .Set(scheme => scheme.IsObsolete, scheme.IsObsolete)
                .Set(scheme => scheme.TenantId, scheme.TenantId);

            await dbcoll.UpdateOneAsync(filter, update, new UpdateOptions() { IsUpsert = true }).ConfigureAwait(false);
        }

        public virtual async Task SaveSchemeAsync(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags, string tenantId = null)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);

            FilterDefinition<WorkflowScheme> filter = GetWorkflowSchemeExactFilter(schemaCode, tenantId);
            WorkflowScheme existing = await (await dbcoll.FindAsync(filter).ConfigureAwait(false))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            var workflowScheme = new WorkflowScheme
            {
                Id = existing?.Id ?? Guid.NewGuid().ToString("N"),
                Code = schemaCode,
                Scheme = scheme,
                CanBeInlined = canBeInlined,
                InlinedSchemes = inlinedSchemes,
                Tags = tags,
                TenantId = tenantId
            };

            await dbcoll.ReplaceOneAsync(filter, workflowScheme, new ReplaceOptions { IsUpsert = true })
                .ConfigureAwait(false);
        }

        public virtual async Task<XElement> GetSchemeAsync(string code, string tenantId = null)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme scheme = await GetWorkflowSchemeAsync(dbcoll, code, tenantId).ConfigureAwait(false);

            if (scheme == null || String.IsNullOrEmpty(scheme.Scheme))
            {
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);
            }

            return XElement.Parse(scheme.Scheme);
        }

        public virtual async Task<List<string>> GetInlinedSchemeCodesAsync(string tenantId = null)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            FilterDefinitionBuilder<WorkflowScheme> filterBuilder = Builders<WorkflowScheme>.Filter;
            FilterDefinition<WorkflowScheme> filter = filterBuilder.Eq(n => n.CanBeInlined, true);

            if (tenantId == null)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(scheme => scheme.TenantId, null));
            }
            else
            {
                filter = filterBuilder.And(
                    filter,
                    filterBuilder.Or(
                        filterBuilder.Eq(scheme => scheme.TenantId, tenantId),
                        filterBuilder.Eq(scheme => scheme.TenantId, null)));
            }

            ProjectionDefinition<WorkflowScheme> projection = Builders<WorkflowScheme>.Projection
                .Include(b => b.Code)
                .Exclude("_id");
            var options = new FindOptions<WorkflowScheme, BsonDocument> {Projection = projection};
            var codes = (await (await dbcoll.FindAsync(filter, options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString)
                .Distinct()
                .ToList();
            return codes;
        }

        public virtual async Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode, string tenantId = null)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            FilterDefinition<WorkflowScheme> filter = Builders<WorkflowScheme>.Filter.AnyEq(sch => sch.InlinedSchemes, schemeCode);
            if (tenantId != null)
            {
                filter = Builders<WorkflowScheme>.Filter.And(
                    filter,
                    Builders<WorkflowScheme>.Filter.Eq(sch => sch.TenantId, tenantId));
            }

            ProjectionDefinition<WorkflowScheme> projection = Builders<WorkflowScheme>.Projection
                .Include(b => b.Code)
                .Exclude("_id");
            var options = new FindOptions<WorkflowScheme, BsonDocument> {Projection = projection};
            var codes = (await (await dbcoll.FindAsync(filter, options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString)
                .Distinct()
                .ToList();
            return codes;
        }

        public virtual async Task<List<string>> SearchSchemesByTagsAsync(params string[] tags)
        {
            return await SearchSchemesByTagsAsync(tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags)
        {
            return await SearchSchemesByTagsAsync(null, tags).ConfigureAwait(false);
        }

        public virtual async Task<List<string>> SearchSchemesByTagsAsync(string tenantId, IEnumerable<string> tags)
        {
            var tagsList = tags?.ToList();
            bool isEmpty = tagsList == null || !tagsList.Any();
            
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);

            FilterDefinitionBuilder<WorkflowScheme> filterBuilder = Builders<WorkflowScheme>.Filter;
            FilterDefinition<WorkflowScheme> tenantFilter = tenantId == null
                ? filterBuilder.Eq(scheme => scheme.TenantId, null)
                : filterBuilder.Eq(scheme => scheme.TenantId, tenantId);

            ProjectionDefinition<WorkflowScheme> projection = Builders<WorkflowScheme>.Projection
                .Include(b => b.Code)
                .Exclude("_id");
            
            var options = new FindOptions<WorkflowScheme, BsonDocument> {Projection = projection};

            if (!isEmpty)
            {
                var tagFilters = new List<FilterDefinition<WorkflowScheme>>();
                foreach (string tag in tagsList)
                {
                    tagFilters.Add(filterBuilder.AnyEq(s => s.Tags, tag));
                }

                FilterDefinition<WorkflowScheme> filter = filterBuilder.And(tenantFilter, filterBuilder.Or(tagFilters));

                return (await (await dbcoll.FindAsync(filter, options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false))
                    .Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString).ToList();
            }

            return (await (await dbcoll.FindAsync(tenantFilter, options).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false))
                .Select(d => d.GetValue(nameof(WorkflowScheme.Code)).AsString).ToList();
        }

        public virtual async Task<List<string>> SearchSchemesByTagsInTenantAsync(string tenantId, params string[] tags)
        {
            return await SearchSchemesByTagsAsync(tenantId, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task AddSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await AddSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await AddSchemeTagsAsync(schemeCode, null, tags).ConfigureAwait(false);
        }

        public virtual async Task AddSchemeTagsAsync(string schemeCode, string tenantId, IEnumerable<string> tags)
        {
            await UpdateSchemeTagsAsync(schemeCode, tenantId, schemeTags => tags.Concat(schemeTags).ToList()).ConfigureAwait(false);
        }

        public virtual async Task AddSchemeTagsInTenantAsync(string schemeCode, string tenantId, params string[] tags)
        {
            await AddSchemeTagsAsync(schemeCode, tenantId, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task RemoveSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await RemoveSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await RemoveSchemeTagsAsync(schemeCode, null, tags).ConfigureAwait(false);
        }

        public virtual async Task RemoveSchemeTagsAsync(string schemeCode, string tenantId, IEnumerable<string> tags)
        {
            await UpdateSchemeTagsAsync(schemeCode, tenantId, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList()).ConfigureAwait(false);
        }

        public virtual async Task RemoveSchemeTagsInTenantAsync(string schemeCode, string tenantId, params string[] tags)
        {
            await RemoveSchemeTagsAsync(schemeCode, tenantId, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeTagsAsync(string schemeCode, params string[] tags)
        {
            await SetSchemeTagsAsync(schemeCode, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            await SetSchemeTagsAsync(schemeCode, null, tags).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeTagsAsync(string schemeCode, string tenantId, IEnumerable<string> tags)
        {
            await UpdateSchemeTagsAsync(schemeCode, tenantId, schemeTags => tags.ToList()).ConfigureAwait(false);
        }

        public virtual async Task SetSchemeTagsInTenantAsync(string schemeCode, string tenantId, params string[] tags)
        {
            await SetSchemeTagsAsync(schemeCode, tenantId, tags?.AsEnumerable()).ConfigureAwait(false);
        }

        private async Task UpdateSchemeTagsAsync(string schemeCode, string tenantId, Func<List<string>, List<string>> getNewTags)
        {
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme scheme = await (await dbcoll.FindAsync(GetWorkflowSchemeExactFilter(schemeCode, tenantId)).ConfigureAwait(false))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

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

        public virtual async Task<XElement> GenerateAsync(string schemeCode, string tenantId = null)
        {
            string code = !_templateTypeMapping.ContainsKey(schemeCode.ToLower()) ? schemeCode : _templateTypeMapping[schemeCode.ToLower()];

            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);

            WorkflowScheme scheme = await GetWorkflowSchemeAsync(dbcoll, code, tenantId).ConfigureAwait(false);


            if (scheme == null)
            {
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowProcessScheme);
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
            IQueryable<WorkflowInbox> query = dCollection.AsQueryable()
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

            IQueryable<WorkflowInbox> query = dCollection.AsQueryable()
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
                        && h.TransitionTime == null
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
                        .Set(x => x.TransitionTime, approvalHistoryItem.TransitionTime)
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
            
            IQueryable<WorkflowApprovalHistory> query = dCollection.AsQueryable()
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

            IQueryable<WorkflowApprovalHistory> query = dCollection.AsQueryable()
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
        
        #region IFormDataProvider

        public async Task<WorkflowForm> GetFormAsync(string name, int? version = null, string tenantId = null)
        {
            var formCollection = Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);
            Models.WorkflowForm entity = await GetPreferredFormAsync(formCollection, name, version, tenantId).ConfigureAwait(false);
            return entity?.ToModel();
        }

        public async Task<List<string>> GetFormNamesAsync(string tenantId = null)
        {
            var formCollection = Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);
            return await GetScopedDistinctValuesAsync<string>(formCollection, tenantId, nameof(WorkflowFormEntity.Name))
                .ConfigureAwait(false);
        }

        public async Task<List<int>> GetFormVersionsAsync(string name, string tenantId = null)
        {
            var formCollection = Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);
            var filter = Builders<Models.WorkflowForm>.Filter.And(
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Name, name),
                GetAccessibleTenantFilter(tenantId));

            List<Models.WorkflowForm> forms = await formCollection.Find(filter).ToListAsync().ConfigureAwait(false);
            IEnumerable<Models.WorkflowForm> scopedForms = tenantId is not null && forms.Any(f => f.TenantId == tenantId)
                ? forms.Where(f => f.TenantId == tenantId)
                : forms.Where(f => f.TenantId == null);

            return scopedForms.Select(f => f.Version).Distinct().ToList();
        }

        public async Task<WorkflowForm> CreateNewFormVersionAsync(string name, string defaultDefinition, int? version = null,
            string tenantId = null)
        {
            var formCollection = Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);

            for (int attempt = 0; attempt < CreateNewFormVersionMaxAttempts; attempt++)
            {
                DateTime now = _runtime.RuntimeDateTimeNow;
                Models.WorkflowForm latestInTenantScope = await GetExactScopeLatestFormAsync(formCollection, name, tenantId)
                    .ConfigureAwait(false);
                int newVersion = latestInTenantScope != null ? latestInTenantScope.Version + 1 : 0;

                Models.WorkflowForm sourceForm = version is null
                    ? latestInTenantScope
                    : await GetExactScopeFormAsync(formCollection, name, version, tenantId).ConfigureAwait(false);

                if (sourceForm is null && tenantId is not null && latestInTenantScope is null)
                {
                    sourceForm = version is null
                        ? await GetExactScopeLatestFormAsync(formCollection, name, tenantId: null).ConfigureAwait(false)
                        : await GetExactScopeFormAsync(formCollection, name, version, tenantId: null).ConfigureAwait(false);
                }

                if (version is not null && sourceForm is null)
                {
                    throw new InvalidOperationException("The form with the specified name and version was not found.");
                }

                string definition = sourceForm?.Definition ?? defaultDefinition;

                var entity = new Models.WorkflowForm
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Version = newVersion,
                    CreationDate = now,
                    UpdatedDate = now,
                    Definition = definition,
                    Lock = 0,
                    TenantId = tenantId
                };

                try
                {
                    await formCollection.InsertOneAsync(entity).ConfigureAwait(false);
                    return entity.ToModel();
                }
                catch (MongoWriteException ex) when (ex.IsDuplicateKeyException())
                {
                    if (attempt == CreateNewFormVersionMaxAttempts - 1)
                    {
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(CreateNewFormVersionRetryDelayMilliseconds * (attempt + 1)))
                        .ConfigureAwait(false);
                }
            }

            throw new Exception("Unable to create a new form version.");
        }

        public async Task<WorkflowForm> CreateNewFormIfNotExistsAsync(string name, string defaultDefinition, string tenantId = null)
        {
            IMongoCollection<Models.WorkflowForm> formCollection =
                Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);

            Models.WorkflowForm existing = await GetExactScopeFormAsync(formCollection, name, version: null, tenantId).ConfigureAwait(false);
            if (existing != null)
            {
                return existing.ToModel();
            }

            string definition = defaultDefinition;
            if (tenantId is not null)
            {
                Models.WorkflowForm sharedForm = await GetExactScopeFormAsync(formCollection, name, version: null, tenantId: null)
                    .ConfigureAwait(false);
                definition = sharedForm?.Definition ?? defaultDefinition;
            }

            FilterDefinition<Models.WorkflowForm> filter = Builders<Models.WorkflowForm>.Filter.And(
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Name, name),
                GetExactTenantFilter(tenantId));

            DateTime now = _runtime.RuntimeDateTimeNow;

            UpdateDefinition<Models.WorkflowForm> update = Builders<Models.WorkflowForm>.Update
                .SetOnInsert(f => f.Id, Guid.NewGuid())
                .SetOnInsert(f => f.Name,name)
                .SetOnInsert(f => f.Version, 0)
                .SetOnInsert(f => f.CreationDate, now)
                .SetOnInsert(f => f.UpdatedDate, now)
                .SetOnInsert(f => f.Definition, definition)
                .SetOnInsert(f => f.Lock, 0)
                .SetOnInsert(f => f.TenantId, tenantId);

            var options = new UpdateOptions { IsUpsert = true };

            try
            {
                await formCollection.UpdateOneAsync(filter, update, options).ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.IsDuplicateKeyException())
            {
                Models.WorkflowForm existingAfterDuplicate =
                    await GetExactScopeFormAsync(formCollection, name, version: null, tenantId).ConfigureAwait(false);
                if (existingAfterDuplicate != null)
                {
                    return existingAfterDuplicate.ToModel();
                }

                throw;
            }

            Models.WorkflowForm resultForm = await formCollection.Find(filter)
                .SortByDescending(f => f.Version)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (resultForm == null)
            {
                throw new Exception("Unable to create a new form.");
            }

            return resultForm.ToModel();
        }

        public async Task<int> UpdateFormAsync(string name, int version, int lockValue, string definition, string tenantId = null)
        {
            var formCollection = Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);
            var filter = Builders<Models.WorkflowForm>.Filter.And(
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Name, name),
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Version, version),
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Lock, lockValue),
                GetExactTenantFilter(tenantId)
            );

            var newLockValue = lockValue == int.MaxValue ? 0 : lockValue + 1;
            DateTime now = _runtime.RuntimeDateTimeNow;
            var update = Builders<Models.WorkflowForm>.Update
                .Set(f => f.Definition, definition)
                .Set(f => f.UpdatedDate, now)
                .Set(f => f.Lock, newLockValue);

            var result = await formCollection.UpdateOneAsync(filter, update).ConfigureAwait(false);

            if (result.ModifiedCount != 1)
            {
                throw new Exception(
                    $"The form '{name}' with version '{version}' was either updated earlier or does not exist. Unable to update the form.");
            }

            return newLockValue;
        }

        public async Task DeleteFormVersionAsync(string name, int version, string tenantId = null)
        {
            var formCollection = Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);
            var filter = Builders<Models.WorkflowForm>.Filter.And(
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Name, name),
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Version, version),
                GetExactTenantFilter(tenantId)
            );

            await formCollection.DeleteOneAsync(filter).ConfigureAwait(false);
        }

        public async Task DeleteFormAsync(string name, string tenantId = null)
        {
            var formCollection = Store.GetCollection<Models.WorkflowForm>(MongoDBConstants.WorkflowFormCollectionName);
            var filter = Builders<Models.WorkflowForm>.Filter.And(
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Name, name),
                GetExactTenantFilter(tenantId));
            await formCollection.DeleteManyAsync(filter).ConfigureAwait(false);
        }
        
        #endregion

        private static async Task<Models.WorkflowForm> GetPreferredFormAsync(IMongoCollection<Models.WorkflowForm> formCollection,
            string name, int? version, string tenantId)
        {
            FilterDefinition<Models.WorkflowForm> filter = Builders<Models.WorkflowForm>.Filter.And(
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Name, name),
                GetAccessibleTenantFilter(tenantId));

            List<Models.WorkflowForm> forms = await formCollection.Find(filter).ToListAsync().ConfigureAwait(false);
            IEnumerable<Models.WorkflowForm> scopedForms = forms;

            if (tenantId is not null)
            {
                scopedForms = forms.Any(f => f.TenantId == tenantId)
                    ? forms.Where(f => f.TenantId == tenantId)
                    : forms.Where(f => f.TenantId == null);
            }

            if (version.HasValue)
            {
                return scopedForms.FirstOrDefault(f => f.Version == version.Value);
            }

            return scopedForms
                .OrderByDescending(f => f.Version)
                .FirstOrDefault();
        }

        private static async Task<Models.WorkflowForm> GetExactScopeFormAsync(IMongoCollection<Models.WorkflowForm> formCollection,
            string name, int? version, string tenantId)
        {
            var filter = Builders<Models.WorkflowForm>.Filter.And(
                Builders<Models.WorkflowForm>.Filter.Eq(f => f.Name, name),
                GetExactTenantFilter(tenantId));

            if (version.HasValue)
            {
                filter = Builders<Models.WorkflowForm>.Filter.And(filter,
                    Builders<Models.WorkflowForm>.Filter.Eq(f => f.Version, version.Value));
            }

            return version.HasValue
                ? await formCollection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false)
                : await formCollection.Find(filter).SortByDescending(f => f.Version).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        private static Task<Models.WorkflowForm> GetExactScopeLatestFormAsync(IMongoCollection<Models.WorkflowForm> formCollection,
            string name, string tenantId)
        {
            return GetExactScopeFormAsync(formCollection, name, version: null, tenantId);
        }

        private static FilterDefinition<Models.WorkflowForm> GetExactTenantFilter(string tenantId)
        {
            return tenantId is null
                ? Builders<Models.WorkflowForm>.Filter.Eq(f => f.TenantId, null)
                : Builders<Models.WorkflowForm>.Filter.Eq(f => f.TenantId, tenantId);
        }

        private static FilterDefinition<Models.WorkflowForm> GetAccessibleTenantFilter(string tenantId)
        {
            return tenantId is null
                ? Builders<Models.WorkflowForm>.Filter.Eq(f => f.TenantId, null)
                : Builders<Models.WorkflowForm>.Filter.Or(
                    Builders<Models.WorkflowForm>.Filter.Eq(f => f.TenantId, tenantId),
                    Builders<Models.WorkflowForm>.Filter.Eq(f => f.TenantId, null));
        }

        private static async Task<List<TValue>> GetScopedDistinctValuesAsync<TValue>(
            IMongoCollection<Models.WorkflowForm> formCollection, string tenantId, string fieldName,
            FilterDefinition<Models.WorkflowForm> baseFilter = null)
        {
            baseFilter ??= FilterDefinition<Models.WorkflowForm>.Empty;

            return await formCollection
                .Distinct<TValue>(fieldName,
                    Builders<Models.WorkflowForm>.Filter.And(baseFilter, GetAccessibleTenantFilter(tenantId)))
                .ToListAsync()
                .ConfigureAwait(false);
        }

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

        private static SortDefinition<T> GetSortDefinition<T>(Sorting sort)
        {
            return sort.SortDirection == SortDirection.Desc
                ? Builders<T>.Sort.Descending(sort.FieldName)
                : Builders<T>.Sort.Ascending(sort.FieldName);
        }
    }
}
