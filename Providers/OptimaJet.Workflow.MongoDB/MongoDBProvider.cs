using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Runtime.Timers;

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

    public class MongoDBProvider : IWorkflowProvider, IApprovalProvider
    {
        private WorkflowRuntime _runtime;
        private readonly bool WriteToHistory;
        private readonly bool WriteSubProcessToRoot;

        public MongoDBProvider(IMongoDatabase store,bool writeToHistory = true, bool writeSubProcessToRoot = false)
        {
            Store = store;
            WriteToHistory = writeToHistory;
            WriteSubProcessToRoot = writeSubProcessToRoot;
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
        public void DeleteInactiveTimersByProcessId(Guid processId)
        {
            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcollTimer.DeleteMany(c => c.ProcessId == processId && c.Ignore);
        }

        public async Task DeleteTimerAsync(Guid timerId)
        {
            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            await dbcollTimer.DeleteOneAsync(x => x.Id == timerId).ConfigureAwait(false);
        }

        public List<Guid> GetRunningProcesses(string runtimeId = null)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

            if(string.IsNullOrEmpty(runtimeId))
            {
                return dbcoll.Find(x => x.Status.Status == ProcessStatus.Running.Id).ToList().Select(x => x.Id).ToList();
            }

            return dbcoll.Find(x => x.Status.Status == ProcessStatus.Running.Id && x.Status.RuntimeId == runtimeId).ToList().Select(x => x.Id).ToList();
        }

        public WorkflowRuntimeModel CreateWorkflowRuntime(string runtimeId, RuntimeStatus status)
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll = 
                Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            var runtime = new Models.WorkflowRuntime()
            {
                RuntimeId = runtimeId,
                Lock = Guid.NewGuid(),
                Status = status
            };

            dbcoll.InsertOne(runtime);

            return new WorkflowRuntimeModel { Lock = runtime.Lock, RuntimeId = runtimeId, Status = status };
            
        }

        public void DeleteWorkflowRuntime(string name)
        {
            IMongoCollection<Models.WorkflowRuntime> dbcoll =
                 Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);
            dbcoll.DeleteOne(x => x.RuntimeId == name);
        }
        
        public WorkflowRuntimeModel UpdateWorkflowRuntimeStatus(WorkflowRuntimeModel runtime, RuntimeStatus status)
        {
            var res = UpdateWorkflowRuntime(runtime, x => x.Status = status, Builders<Models.WorkflowRuntime>.Update.Set(x => x.Status, status));

            if (res.Item1 != 1)
            {
                throw new ImpossibleToSetRuntimeStatusException();
            }

            return res.Item2;
        }

        public (bool Success, WorkflowRuntimeModel UpdatedModel) UpdateWorkflowRuntimeRestorer(WorkflowRuntimeModel runtime, string restorerId)
        {
            var res = UpdateWorkflowRuntime(runtime, x => x.RestorerId = restorerId, Builders<Models.WorkflowRuntime>.Update.Set(x => x.RestorerId, restorerId));

            return (res.Item1 == 1, res.Item2);
        }

        public bool MultiServerRuntimesExist()
        {
            var empty = Guid.Empty.ToString();
            return Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName)
                        .Find(x => x.RuntimeId != empty || x.Status != RuntimeStatus.Single && x.Status != RuntimeStatus.Terminated && x.Status != RuntimeStatus.Dead)
                        .FirstOrDefault() != null;
        }

        public int SingleServerRuntimesCount()
        {
            string empty = Guid.Empty.ToString();
            return (int)Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName)
                             .CountDocuments(x => x.RuntimeId == empty && x.Status == RuntimeStatus.Single);
        }

        public int ActiveMultiServerRuntimesCount(string currentRuntimeId)
        {
            return (int)Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName)
                             .CountDocuments(x => x.RuntimeId != currentRuntimeId && (x.Status == RuntimeStatus.Alive || x.Status == RuntimeStatus.Restore || x.Status == RuntimeStatus.SelfRestore));
        }

        public WorkflowRuntimeModel GetWorkflowRuntimeModel(string runtimeId)
        {
            var result = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName)
                              .Find(x => x.RuntimeId == runtimeId)
                              .FirstOrDefault();

            if (result == null)
            {
                return null;
            }

            return GetModel(result);
        }

        public void InitializeProcess(ProcessInstance processInstance)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var oldProcess = dbcoll.Find(x => x.Id == processInstance.ProcessId).FirstOrDefault();
                
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
                Persistence = new List<WorkflowProcessInstancePersistence>(),
                TenantId = processInstance.TenantId 
            };
            dbcoll.InsertOne(newProcess);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            BindProcessToNewScheme(processInstance, false);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var oldProcess = dbcoll.Find(x => x.Id == processInstance.ProcessId).FirstOrDefault();
            if (oldProcess == null)
                throw new ProcessNotFoundException(processInstance.ProcessId);

            oldProcess.SchemeId = processInstance.SchemeId;
            if (resetIsDeterminingParametersChanged)
                oldProcess.IsDeterminingParametersChanged = false;

            Save(dbcoll, oldProcess, doc => doc.Id == oldProcess.Id);
        }

        private void Save<T>(IMongoCollection<T> collection, T obj, Expression<Func<T, bool>> filter)
        {
            collection.ReplaceOne<T>(filter, obj, new UpdateOptions { IsUpsert = true });
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
                             return new { Parameter = ptp, SerializedValue = (string)ptp.Value };
                         return new { Parameter = ptp, SerializedValue = ParametersSerializer.Serialize(ptp.Value, ptp.Type) };
                     })
                     .ToList();

            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var process = dbcoll.Find(x => x.Id == processInstance.ProcessId).FirstOrDefault();
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

                Save(dbcoll, process, doc => doc.Id == process.Id);
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
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var instance = dbcoll.Find(x => x.Id == processInstance.ProcessId).FirstOrDefault();

            var status = new WorkflowProcessInstanceStatus
            {
                Lock = Guid.NewGuid(),
                Status = ProcessStatus.Initialized.Id,
                RuntimeId = _runtime.Id,
                SetTime = _runtime.RuntimeDateTimeNow
            };

            if (instance.Status == null)
            {
                dbcoll.UpdateOne(x => x.Id == instance.Id, Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, status));
            }
            else
            {
                var oldLock = instance.Status.Lock;

                var result = dbcoll.UpdateOne(x => x.Id == instance.Id && x.Status.Lock == oldLock, 
                    Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, status));

                if(result.ModifiedCount != 1)
                    throw new ImpossibleToSetStatusException();
            }
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
            var time = _runtime.RuntimeDateTimeNow;
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            dbcoll.UpdateMany(item => item.Status.Status == 1, Builders<WorkflowProcessInstance>.Update
                .Set(c => c.Status.Status, 2)
                .Set(c => c.Status.SetTime, time)
            );
        }

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            var identityId = paramIdentityId == null ? string.Empty : (string) paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string) paramImpIdentityId.Value;

            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var inst = dbcoll.Find(x => x.Id == processInstance.ProcessId).FirstOrDefault();
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

                Save(dbcoll, inst, doc => doc.Id == inst.Id);
            }

            if (!WriteToHistory)
                return;

            var history = new WorkflowProcessTransitionHistory
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

            var dbcollTransition = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            dbcollTransition.InsertOne(history);
        }

        public bool IsProcessExists(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            return dbcoll.Find(x => x.Id == processId).FirstOrDefault() != null;
        }

        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var instance = dbcoll.Find(x => x.Id == processId).FirstOrDefault();
            if (instance == null)
                return ProcessStatus.NotFound;
            var status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status?.Status);
            if (status == null)
                return ProcessStatus.Unknown;
            return status;
        }
        
        private void SetCustomStatus(Guid processId, ProcessStatus status)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var instance = dbcoll.Find(x => x.Id == processId).FirstOrDefault();
            if (instance == null)
                throw new StatusNotDefinedException();

            var newStatus = new WorkflowProcessInstanceStatus
            {
                Lock = Guid.NewGuid(),
                Status = status.Id,
                SetTime = _runtime.RuntimeDateTimeNow,
                RuntimeId = _runtime.Id
            };

            if (instance.Status == null)
            {
                dbcoll.UpdateOne(x => x.Id == instance.Id, Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, newStatus));
            }
            else
            {
                var oldLock = instance.Status.Lock;

                var result = dbcoll.UpdateOne(x => x.Id == instance.Id && x.Status.Lock == oldLock,
                    Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, newStatus));

                if (result.ModifiedCount != 1)
                    throw new ImpossibleToSetStatusException();
            }
        }
        
        private void SetRunningStatus(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var instance = dbcoll.Find(x => x.Id == processId).FirstOrDefault();

            if (instance.Status == null)
                throw new StatusNotDefinedException();

            if (instance.Status.Status == ProcessStatus.Running.Id)
                throw new ImpossibleToSetStatusException("Process already running");

            var status = new WorkflowProcessInstanceStatus
            {
                Lock = Guid.NewGuid(),
                Status = ProcessStatus.Running.Id,
                SetTime = _runtime.RuntimeDateTimeNow,
                RuntimeId = _runtime.Id
            };

            var oldLock = instance.Status.Lock;

            var result = dbcoll.UpdateOne(x => x.Id == instance.Id && x.Status.Lock == oldLock,
                Builders<WorkflowProcessInstance>.Update.Set(x => x.Status, status));

            if (result.ModifiedCount != 1)
                throw new ImpossibleToSetStatusException();
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
                    processInstance.TenantId)
            };
            return parameters;
        }

        private IEnumerable<ParameterDefinitionWithValue> GetPersistedProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count());

            List<WorkflowProcessInstancePersistence> persistedParameters;
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var process = dbcoll.Find(x => x.Id == processId).FirstOrDefault();
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
                var parameterDefinition = persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.ParameterName);
                if (parameterDefinition == null)
                {
                    parameterDefinition = ParameterDefinition.Create(persistedParameter.ParameterName, typeof(UnknownParameterType), ParameterPurpose.Persistence);
                    parameters.Add(ParameterDefinition.Create(parameterDefinition,persistedParameter.Value));
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
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            {
                var processInstance = dbcoll.Find(x => x.Id == processId).FirstOrDefault();
                if (processInstance == null)
                    throw new ProcessNotFoundException(processId);
                return processInstance;
            }
        }

        public void DeleteProcess(Guid[] processIds)
        {
            foreach (var processId in processIds)
                DeleteProcess(processId);
        }

        public void SaveGlobalParameter<T>(string type, string name, T value)
        {
            var dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            var parameter = dbcoll.Find(item => item.Type == type && item.Name == name).FirstOrDefault();

            if (parameter == null)
            {
                parameter = new WorkflowGlobalParameter
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Type = type,
                    Value = JsonConvert.SerializeObject(value)
                };

                dbcoll.InsertOne(parameter);
            }
            else
            {
                parameter.Value = JsonConvert.SerializeObject(value);
                Save(dbcoll, parameter, doc => doc.Id == parameter.Id);
            }
        }

        public T LoadGlobalParameter<T>(string type, string name)
        {
            var dbcoll = Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            var parameter = dbcoll.Find(item => item.Type == type && item.Name == name).FirstOrDefault();

            if (parameter != null)
                return JsonConvert.DeserializeObject<T>(parameter.Value);

            return default(T);
        }

        public List<T> LoadGlobalParameters<T>(string type)
        {
            var dbcoll =
                Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            return
                dbcoll.Find(item => item.Type == type)
                      .Project(gp => JsonConvert.DeserializeObject<T>(gp.Value))
                      .ToList();
        }

        public void DeleteGlobalParameters(string type, string name = null)
        {
            var dbcoll =
                Store.GetCollection<WorkflowGlobalParameter>(MongoDBConstants.WorkflowGlobalParameterCollectionName);

            Expression<Func<WorkflowGlobalParameter, bool>> predicate = null;

            if(string.IsNullOrEmpty(name))
            {
                predicate = item => item.Type == type;
            }else
            {
                predicate = item => item.Type == type && item.Name == name;
            }

            dbcoll.DeleteMany(predicate);
        }

        public void DeleteProcess(Guid processId)
        {
            var dbcollInstance = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            dbcollInstance.DeleteOne(c => c.Id == processId);

            var dbcollTransition = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            dbcollTransition.DeleteMany(c => c.ProcessId == processId);

            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcollTimer.DeleteMany(c => c.ProcessId == processId);
        }

        public void RegisterTimer(Guid processId, Guid rootProcessId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var timer = dbcoll.Find(item => item.ProcessId == processId && item.Name == name).FirstOrDefault();
            if (timer == null)
            {
                timer = new WorkflowProcessTimer
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    NextExecutionDateTime = nextExecutionDateTime,
                    ProcessId = processId,
                    RootProcessId = rootProcessId
                };

                timer.Ignore = false;
                dbcoll.InsertOne(timer);
            }
            else if (!notOverrideIfExists)
            {
                timer.NextExecutionDateTime = nextExecutionDateTime;
                Save(dbcoll, timer, doc => doc.Id == timer.Id);
            }
        }

        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcollTimer.DeleteMany(c => c.ProcessId == processId && !timersIgnoreList.Contains(c.Name));
        }

        public void ClearTimerIgnore(Guid timerId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcoll.UpdateMany(item => item.Id == timerId, Builders<WorkflowProcessTimer>.Update.Set(c => c.Ignore, false));
        }

        public int SetTimerIgnore(Guid timerId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var result = dbcoll.UpdateMany(item => item.Id == timerId && !item.Ignore, Builders<WorkflowProcessTimer>.Update.Set(c => c.Ignore, true));
            return (int)result.ModifiedCount;
        }

        public void ClearTimer(Guid timerId)
        {
            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcollTimer.DeleteOne(c => c.Id == timerId);
        }

        public DateTime? GetCloseExecutionDateTime()
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var timer = dbcoll.AsQueryable().Where(item => !item.Ignore).OrderBy(item => item.NextExecutionDateTime).FirstOrDefault();
            if (timer == null)
                return null;

            return _runtime.ToRuntimeTime(timer.NextExecutionDateTime);
        }

        public List<TimerToExecute> GetTimersToExecute()
        {
            var now = _runtime.RuntimeDateTimeNow.ToUniversalTime();
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var timers = dbcoll.Find(item => !item.Ignore && item.NextExecutionDateTime <= now).ToList();
            var selectedIds = timers.Select(t => t.Id).ToList();
            dbcoll.UpdateMany(item => selectedIds.Contains(item.Id), Builders<WorkflowProcessTimer>.Update.Set(c => c.Ignore, true));
            return timers.Select(t => new TimerToExecute {Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id}).ToList();
        }

        public List<Core.Model.WorkflowTimer> GetTopTimersToExecute(int top)
        {
            DateTime now = _runtime.RuntimeDateTimeNow.ToUniversalTime();

            IMongoCollection<WorkflowProcessTimer> timerColl =
                Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);


            IEnumerable<Core.Model.WorkflowTimer> result = timerColl.AsQueryable()
                .Where(x => !x.Ignore && x.NextExecutionDateTime <= now)
                .ToList()
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

        public List<ProcessHistoryItem> GetProcessHistory(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            var history = dbcoll.Find(hi => hi.ProcessId == processId).ToList();
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
                    TriggerName = hi.TriggerName
                })
                .ToList();
        }

        public IEnumerable<ProcessTimer> GetTimersForProcess(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var history = dbcoll.Find(hi => hi.ProcessId == processId).ToList();
            return history.Select(hi => new ProcessTimer(hi.Id, hi.Name, _runtime.ToRuntimeTime(hi.NextExecutionDateTime)));
        }

        public async Task<List<IProcessInstanceTreeItem>> GetProcessInstanceTreeAsync(Guid rootProcessId)
        {
            IMongoCollection<WorkflowProcessInstance> workflowProcessInstanceCollection =
                Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);

            ProjectionDefinition<WorkflowProcessInstance> workflowProcessInstanceProjection = Builders<WorkflowProcessInstance>.Projection
                .Include(pi => pi.Id)
                .Include(pi => pi.ParentProcessId)
                .Include(pi => pi.RootProcessId)
                .Include(pi => pi.SchemeId);

            var workflowProcessInstanceOptions = new FindOptions<WorkflowProcessInstance, BsonDocument> {Projection = workflowProcessInstanceProjection};

            FilterDefinition<WorkflowProcessInstance> workflowProcessInstanceFilter =
                Builders<WorkflowProcessInstance>.Filter.Eq(pi => pi.RootProcessId, rootProcessId);

            var instances = await (await workflowProcessInstanceCollection.FindAsync(workflowProcessInstanceFilter, workflowProcessInstanceOptions)
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

        public IEnumerable<ProcessTimer> GetActiveTimersForProcess(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var history = dbcoll.Find(hi => hi.ProcessId == processId && !hi.Ignore).ToList();
            return history.Select(hi => new ProcessTimer(hi.Id, hi.Name, _runtime.ToRuntimeTime(hi.NextExecutionDateTime)));
        }

        public int SendRuntimeLastAliveSignal()
        {
            var dbcoll = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            var time = _runtime.RuntimeDateTimeNow;
            var id = _runtime.Id;

            var result = dbcoll.UpdateOne(x => (x.Status == RuntimeStatus.Alive || x.Status == RuntimeStatus.SelfRestore) && x.RuntimeId == id,
                Builders<Models.WorkflowRuntime>.Update.Set(x => x.LastAliveSignal, time));

            return (int)result.ModifiedCount;
        }

        public DateTime? GetNextTimerDate(TimerCategory timerCategory, int timerInterval)
        {
            string timerCategoryName = timerCategory.ToString();
            IMongoCollection<Models.WorkflowSync> lockColl = Store.GetCollection<Models.WorkflowSync>(MongoDBConstants.WorkflowSyncCollectionName);
            Models.WorkflowSync sync = lockColl.Find(x => x.Name == timerCategoryName).FirstOrDefault();

            if (sync == null)
            {
                throw new Exception($"Sync lock {timerCategoryName} not found");
            }

            Guid syncLock = sync.Lock;

            IMongoCollection<Models.WorkflowRuntime> runtimeColl = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            string runtimeId = _runtime.Id;

            Expression<Func<Models.WorkflowRuntime, object>> getterExpression = null;
            Func<Models.WorkflowRuntime, DateTime?> getterFunction = null;

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

            Models.WorkflowRuntime max = runtimeColl.Find(x => x.RuntimeId != runtimeId && x.Status == RuntimeStatus.Alive).SortByDescending(getterExpression)
                .Limit(1).FirstOrDefault();

            DateTime result = _runtime.RuntimeDateTimeNow;

            if(max != null && _runtime.ToRuntimeTime(getterFunction(max)) > result)
            {
                result = _runtime.ToRuntimeTime(getterFunction(max)).Value;
            }

            result += TimeSpan.FromMilliseconds(timerInterval);

            using (IClientSessionHandle session = Store.Client.StartSession())
            {
                session.StartTransaction();

                var newLock = Guid.NewGuid();

                runtimeColl.UpdateOne(x => x.RuntimeId == runtimeId, Builders<Models.WorkflowRuntime>.Update.Set(getterExpression, result));

                UpdateResult lockUpdateResult = lockColl.UpdateOne(x => x.Lock == syncLock && x.Name == timerCategoryName, 
                    Builders<Models.WorkflowSync>.Update.Set(c => c.Lock, newLock));

                if(lockUpdateResult.ModifiedCount == 0)
                {
                    session.AbortTransaction();

                    return null;
                }

                session.CommitTransaction();
            }

            return result;
        }

        public List<WorkflowRuntimeModel> GetWorkflowRuntimes()
        {
            IMongoCollection<Models.WorkflowRuntime> runtimeColl = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);
            return runtimeColl.Find(Builders<Models.WorkflowRuntime>.Filter.Empty).ToList().Select(GetModel).ToList();
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

        public IApprovalProvider GetIApprovalProvider()
        {
            return this;
        }

        #endregion

        #region ISchemePersistenceProvider

        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            WorkflowProcessInstance processInstance;
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            {
                processInstance = dbcoll.Find(x => x.Id == processId).FirstOrDefault();
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
            WorkflowProcessScheme processScheme;
            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            {
                processScheme = dbcoll.Find(x => x.Id == schemeId).FirstOrDefault();
            }

            if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

            return ConvertToSchemeDefinition(processScheme);
        }


        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, string definingParameters,
            Guid? rootSchemeId, bool ignoreObsolete)
        {
            var hash = HashHelper.GenerateStringHash(definingParameters);

            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            IEnumerable<WorkflowProcessScheme> processSchemes = ignoreObsolete
                ? dbcoll.Find(
                        pss =>
                            pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash &&
                            pss.RootSchemeId == rootSchemeId &&
                            !pss.IsObsolete)
                    .ToList()
                : dbcoll.Find(
                        pss =>
                            pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash &&
                            pss.RootSchemeId == rootSchemeId).ToList();

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

            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            dbcoll.UpdateMany(
                    item => (item.SchemeCode == schemeCode || item.RootSchemeCode == schemeCode) && item.DefiningParametersHash == definingParametersHash,
                    Builders<WorkflowProcessScheme>.Update.Set(c => c.IsObsolete, true));
        }

        public void SetSchemeIsObsolete(string schemeCode)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            dbcoll.UpdateMany(item => item.SchemeCode == schemeCode || item.RootSchemeCode == schemeCode,
                Builders<WorkflowProcessScheme>.Update.Set(c => c.IsObsolete, true));
        }

        public SchemeDefinition<XElement> SaveScheme(SchemeDefinition<XElement> scheme)
        {
            var definingParameters = scheme.DefiningParameters;
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);

            var oldSchemes =
                dbcoll.Find(
                        wps => wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == scheme.SchemeCode &&
                               wps.IsObsolete == scheme.IsObsolete)
                    .ToList();

            if (oldSchemes.Any())
            {
                var existing = oldSchemes.FirstOrDefault(oldScheme => oldScheme.DefiningParameters == definingParameters);

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

            dbcoll.InsertOne(newProcessScheme);

            return ConvertToSchemeDefinition(newProcessScheme);
        }

        public void SaveScheme(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags)
        {
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            var wfScheme = dbcoll.Find(c => c.Code == schemaCode).FirstOrDefault();

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
                dbcoll.InsertOne(wfScheme);
            }
            else
            {
                wfScheme.Scheme = scheme;
                wfScheme.InlinedSchemes = inlinedSchemes;
                wfScheme.CanBeInlined = canBeInlined;
                wfScheme.Tags = tags;
                Save(dbcoll, wfScheme, doc => doc.Id == wfScheme.Id);
            }
        }

        public XElement GetScheme(string code)
        {
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            var scheme = dbcoll.Find(c => c.Code == code).FirstOrDefault();

            if (scheme == null || string.IsNullOrEmpty(scheme.Scheme))
                throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);

            return XElement.Parse(scheme.Scheme);
        }
        
        public List<string> GetInlinedSchemeCodes()
        {
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            var codes = dbcoll.Find(c => c.CanBeInlined).Project(sch => sch.Code).ToList();
            return codes;
        }

        public List<string> GetRelatedByInliningSchemeCodes(string schemeCode)
        {
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            var filter = Builders<WorkflowScheme>.Filter.AnyEq(sch => sch.InlinedSchemes, schemeCode);
            var codes = dbcoll.Find(filter).Project(sch => sch.Code).ToList();
            return codes;
        }

        public List<string> SearchSchemesByTags(params string[] tags)
        {
            return SearchSchemesByTags(tags?.AsEnumerable());
        }

        public List<string> SearchSchemesByTags(IEnumerable<string> tags)
        {
            bool isEmpty = tags == null || !tags.Any();
            
            IMongoCollection<WorkflowScheme> dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);

            var filters = new List<FilterDefinition<WorkflowScheme>>();

            if (!isEmpty)
            {
                foreach (string tag in tags)
                {
                    filters.Add(Builders<WorkflowScheme>.Filter.AnyEq(s => s.Tags, tag));
                }

                return dbcoll.Find(Builders<WorkflowScheme>.Filter.Or(filters)).Project(sch => sch.Code).ToList();
            }

            return dbcoll.Find(Builders<WorkflowScheme>.Filter.Empty).Project(sch => sch.Code).ToList();
        }

        public void AddSchemeTags(string schemeCode, params string[] tags)
        {
            AddSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public void AddSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            UpdateSchemeTags(schemeCode, (schemeTags) => tags.Concat(schemeTags).ToList());
        }

        public void RemoveSchemeTags(string schemeCode, params string[] tags)
        {
            RemoveSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public void RemoveSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            UpdateSchemeTags(schemeCode,schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList());
        }

        public void SetSchemeTags(string schemeCode, params string[] tags)
        {
            SetSchemeTags(schemeCode, tags?.AsEnumerable());
        }

        public void SetSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            UpdateSchemeTags(schemeCode,
                (schemeTags) => tags.ToList());
        }

        private void UpdateSchemeTags(string schemeCode, Func<List<string>, List<string>> getNewTags)
        {
            IMongoCollection<WorkflowScheme> dbcoll =
                Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme scheme = dbcoll.Find(c => c.Code == schemeCode).FirstOrDefault();

            if (scheme == null)
            {
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowScheme);
            }

            List<string> newTags = getNewTags.Invoke(scheme.Tags);

            scheme.Scheme = _runtime.Builder.ReplaceTagsInScheme(scheme.Scheme,newTags);
            scheme.Tags = newTags;

            Save(dbcoll, scheme, doc => doc.Id == scheme.Id);
        }

        #endregion

        #region IWorkflowGenerator

        protected IDictionary<string, string> TemplateTypeMapping = new Dictionary<string, string>();

        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
                throw new InvalidOperationException("Parameters not supported");

            var code = !TemplateTypeMapping.ContainsKey(schemeCode.ToLower()) ? schemeCode : TemplateTypeMapping[schemeCode.ToLower()];
            WorkflowScheme scheme;
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            {
                scheme = dbcoll.Find(c => c.Code == code).FirstOrDefault();
            }

            if (scheme == null)
                throw new InvalidOperationException(string.Format("Scheme with Code={0} not found", code));

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

        private Tuple<long, WorkflowRuntimeModel> UpdateWorkflowRuntime(WorkflowRuntimeModel runtime, Action<WorkflowRuntimeModel> setter,
            UpdateDefinition<Models.WorkflowRuntime> updater)
        {
            var dbcoll = Store.GetCollection<Models.WorkflowRuntime>(MongoDBConstants.WorkflowRuntimeCollectionName);

            Guid oldLock = runtime.Lock;
            runtime.Lock = Guid.NewGuid();
            setter(runtime);

            UpdateResult result = dbcoll.UpdateOne(x => x.RuntimeId == runtime.RuntimeId && x.Lock == oldLock,
                updater.Set(x => x.Lock, runtime.Lock)
            );

            if (result.MatchedCount != 1)
            {
                return new Tuple<long, WorkflowRuntimeModel>(result.ModifiedCount, null);
            }

            return new Tuple<long, WorkflowRuntimeModel>(result.ModifiedCount, runtime);
        }

        #region IApprovalProvider

        public  async Task DropWorkflowInboxAsync(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            await dbcoll.DeleteOneAsync(c => c.ProcessId == processId).ConfigureAwait(false);
        }

        public  async Task InsertInboxAsync(Guid processId, List<string> newActors)
        {
            var dbcoll = Store.GetCollection<WorkflowInbox>(MongoDBConstants.WorkflowInboxCollectionName);
            var inboxItems = newActors.Select(newactor => new WorkflowInbox() { Id = Guid.NewGuid(), IdentityId = newactor, ProcessId = processId }).ToArray();
            if (inboxItems.Any())
            {
                await dbcoll.InsertManyAsync(inboxItems).ConfigureAwait(false);
            }
        }

        public  async Task WriteApprovalHistoryAsync(Guid id, string currentState, string nextState, string triggerName, string allowedToEmployeeNames, long order)
        {
            var dbcoll = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
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
            await dbcoll.InsertOneAsync(historyItem).ConfigureAwait(false);
        }

        public  async Task UpdateApprovalHistoryAsync(Guid id, string currentState, string nextState, string triggerName, string identityId, long order, string comment)
        {
            var dbcoll = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            var historyItem = dbcoll.Find(h => h.ProcessId == id && !h.TransitionTime.HasValue &&
            h.InitialState == currentState && h.DestinationState == nextState).FirstOrDefault();

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
                    TransitionTime =_runtime.RuntimeDateTimeNow,
                    IdentityId = identityId
                };

                await dbcoll.InsertOneAsync(historyItem).ConfigureAwait(false);
            }
            else
            {
               await dbcoll.UpdateOneAsync(x => x.Id == historyItem.Id,
                   Builders<WorkflowApprovalHistory>.Update.Set(x => x.TriggerName, triggerName).Set(x => x.TransitionTime, _runtime.RuntimeDateTimeNow)
                       .Set(x => x.IdentityId, identityId).Set(x => x.Commentary, comment)).ConfigureAwait(false);
            }

        }

        public async Task DropEmptyApprovalHistoryAsync(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowApprovalHistory>(MongoDBConstants.WorkflowApprovalHistoryCollectionName);
            await dbcoll.DeleteManyAsync(h => h.ProcessId == processId && !h.TransitionTime.HasValue).ConfigureAwait(false);
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
    }
}
