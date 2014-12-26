using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Xml.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Generator;
using MongoDB.Driver;
using OptimaJet.Workflow.MongoDB;
using MongoDB.Driver.Builders;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.DbPersistence
{
    public class MongoDBConstants
    {
        public const string WorkflowProcessInstanceCollectionName = "WorkflowProcessInstance";
        public const string WorkflowProcessInstancePersistenceCollectionName = "WorkflowProcessInstancePersistence";
        public const string WorkflowProcessInstanceStatusCollectionName = "WorkflowProcessInstanceStatus";
        public const string WorkflowProcessSchemeCollectionName = "WorkflowProcessScheme";
        public const string WorkflowProcessTransitionHistoryCollectionName = "WorkflowProcessTransitionHistory";
        public const string WorkflowSchemeCollectionName = "WorkflowScheme";
        public const string WorkflowProcessTimerCollectionName = "WorkflowProcessTimer";
    }

    public class MongoDBProvider : IPersistenceProvider, ISchemePersistenceProvider<XElement>, IWorkflowGenerator<XElement>
    {
        public MongoDatabase Store { get; set; }

        private Core.Runtime.WorkflowRuntime _runtime;
        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public MongoDBProvider(MongoDatabase store)
        {
            Store = store;
        }

        #region IPersistenceProvider
        public void InitializeProcess(ProcessInstance processInstance)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var oldProcess = dbcoll.FindOneById(processInstance.ProcessId);
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
            dbcoll.Insert(newProcess);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            BindProcessToNewScheme(processInstance, false);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var oldProcess = dbcoll.FindOneById(processInstance.ProcessId);
            if (oldProcess == null)
                throw new ProcessNotFoundException();

            oldProcess.SchemeId = processInstance.SchemeId;
            if (resetIsDeterminingParametersChanged)
                oldProcess.IsDeterminingParametersChanged = false;
            dbcoll.Save(oldProcess);
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
              processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence).Select(ptp => new { Parameter = ptp, SerializedValue = _runtime.SerializeParameter(ptp.Value,ptp.Type) })
                  .ToList();
            var persistenceParameters = processInstance.ProcessScheme.PersistenceParameters.ToList();

            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            var process = dbcoll.FindOneById(processInstance.ProcessId);
            if (process != null && process.Persistence != null)
            {
                var persistedParameters = process.Persistence.Where(
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

                dbcoll.Save(process);
            }
        }

        public void SetWorkflowIniialized(ProcessInstance processInstance)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstanceStatus>(MongoDBConstants.WorkflowProcessInstanceStatusCollectionName);
            var instanceStatus = dbcoll.FindOneById(processInstance.ProcessId);
            if (instanceStatus == null)
            {
                instanceStatus = new WorkflowProcessInstanceStatus()
                {
                    Id = processInstance.ProcessId,
                    Lock = Guid.NewGuid(),
                    Status = ProcessStatus.Initialized.Id
                };

                dbcoll.Insert(instanceStatus);

            }
            else
            {
                instanceStatus.Status = ProcessStatus.Initialized.Id;
                dbcoll.Save(instanceStatus);
            }
        }

        public void SetWorkflowIdled(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Idled);
        }

        public void SetWorkflowRunning(ProcessInstance processInstance)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstanceStatus>(MongoDBConstants.WorkflowProcessInstanceStatusCollectionName);
            var instanceStatus = dbcoll.FindOneById(processInstance.ProcessId);
            if (instanceStatus == null)
                throw new StatusNotDefinedException();
            
            if (instanceStatus.Status == ProcessStatus.Running.Id)
                throw new ImpossibleToSetStatusException();

            instanceStatus.Lock = Guid.NewGuid();
            instanceStatus.Status = ProcessStatus.Running.Id;
            dbcoll.Save(instanceStatus);
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
            var dbcoll = Store.GetCollection<WorkflowProcessInstanceStatus>(MongoDBConstants.WorkflowProcessInstanceStatusCollectionName);
            dbcoll.Update(Query<WorkflowProcessInstanceStatus>.EQ(item => item.Status, 1), Update<WorkflowProcessInstanceStatus>.Set(c=> c.Status, 2));
        }

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            var identityId = paramIdentityId == null ? string.Empty : (string)paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string)paramImpIdentityId.Value;

            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            WorkflowProcessInstance inst = dbcoll.FindOneById(processInstance.ProcessId);
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

                dbcoll.Save(inst);
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

            var dbcollTransition = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            dbcollTransition.Insert(history);
        }

        public bool IsProcessExists(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            return dbcoll.FindOneById(processId) != null;
        }

        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstanceStatus>(MongoDBConstants.WorkflowProcessInstanceStatusCollectionName);
            var instance = dbcoll.FindOneById(processId);
            if (instance == null)
                return ProcessStatus.NotFound;
            var status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status);
            if (status == null)
                return ProcessStatus.Unknown;
            return status;
        }


        private void SetCustomStatus(Guid processId, ProcessStatus status)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessInstanceStatus>(MongoDBConstants.WorkflowProcessInstanceStatusCollectionName);
            var instanceStatus = dbcoll.FindOneById(processId);
            if (instanceStatus == null)
                throw new StatusNotDefinedException();
            instanceStatus.Status = status.Id;

            dbcoll.Save(instanceStatus);
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

            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            {
                var process = dbcoll.FindOneById(processId);
                if(process != null && process.Persistence != null)
                {
                    persistedParameters = process.Persistence.Where(WorkflowProcessInstancep => persistenceParameters.Select(pp => pp.Name).Contains(WorkflowProcessInstancep.ParameterName)).ToList();
                }
                else
                {
                    persistedParameters = new List<WorkflowProcessInstancePersistence>();
                }  
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
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            {
                var processInstance = dbcoll.FindOneById(processId);
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
            var dbcollInstance = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            dbcollInstance.Remove(Query<WorkflowProcessInstance>.Where(c => c.Id == processId));

            var dbcollStatus = Store.GetCollection<WorkflowProcessInstanceStatus>(MongoDBConstants.WorkflowProcessInstanceStatusCollectionName);
            dbcollStatus.Remove(Query<WorkflowProcessInstanceStatus>.Where(c => c.Id == processId));

            var dbcollTransition = Store.GetCollection<WorkflowProcessTransitionHistory>(MongoDBConstants.WorkflowProcessTransitionHistoryCollectionName);
            dbcollTransition.Remove(Query<WorkflowProcessTransitionHistory>.Where(c => c.ProcessId == processId));

            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcollTimer.Remove(Query<WorkflowProcessTimer>.Where(c => c.ProcessId == processId));
        }

        public void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var timer = dbcoll.FindOne(Query<WorkflowProcessTimer>.Where(item => item.ProcessId == processId && item.Name == name));
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
                dbcoll.Insert(timer);
            }

            if (!notOverrideIfExists)
            {
                timer.NextExecutionDateTime = nextExecutionDateTime;
                dbcoll.Save(timer);
            }
        }

        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcollTimer.Remove(Query<WorkflowProcessTimer>.Where(c => c.ProcessId == processId && !timersIgnoreList.Contains(c.Name)));
        }

        public void ClearTimersIgnore()
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcoll.Update(
                Query<WorkflowProcessTimer>.Where(item => item.Ignore),
                Update<WorkflowProcessTimer>.Set(c => c.Ignore, false));
        }

        public void ClearTimer(Guid timerId)
        {
            var dbcollTimer = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            dbcollTimer.Remove(Query<WorkflowProcessTimer>.Where(c => c.Id == timerId));
        }

        public DateTime? GetCloseExecutionDateTime()
        {
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var timer = dbcoll.Find(Query<WorkflowProcessTimer>.Where(item => !item.Ignore)).OrderBy(item => item.NextExecutionDateTime).FirstOrDefault();
            if (timer == null)
                return null;

            return timer.NextExecutionDateTime;
        }

        public List<TimerToExecute> GetTimersToExecute()
        {
            var now = _runtime.RuntimeDateTimeNow;
            var dbcoll = Store.GetCollection<WorkflowProcessTimer>(MongoDBConstants.WorkflowProcessTimerCollectionName);
            var timers = dbcoll.Find(Query<WorkflowProcessTimer>.Where(item => !item.Ignore && item.NextExecutionDateTime <= now)).ToArray();
            dbcoll.Update(
                Query<WorkflowProcessTimer>.Where(item => !item.Ignore && item.NextExecutionDateTime <= now),
                Update<WorkflowProcessTimer>.Set(c => c.Ignore, true));

            return timers.Select(t => new TimerToExecute() { Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id }).ToList();            
        }
        #endregion 

        #region ISchemePersistenceProvider
        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            WorkflowProcessInstance processInstance = new WorkflowProcessInstance();
            var dbcoll = Store.GetCollection<WorkflowProcessInstance>(MongoDBConstants.WorkflowProcessInstanceCollectionName);
            {
                processInstance = dbcoll.FindOneById(processId);
            }

            if (processInstance == null)
                throw new ProcessNotFoundException();

            if (!processInstance.SchemeId.HasValue)
                throw new SchemeNotFoundException();
            var schemeDefinition = GetProcessSchemeBySchemeId(processInstance.SchemeId.Value);
            schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
            return schemeDefinition;
        }

        public SchemeDefinition<XElement> GetProcessSchemeBySchemeId(Guid schemeId)
        {
            WorkflowProcessScheme processScheme = new WorkflowProcessScheme();
            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            {
                processScheme = dbcoll.FindOneById(schemeId);
            }

            if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                throw new SchemeNotFoundException();

            return new SchemeDefinition<XElement>(schemeId, XElement.Parse(processScheme.Scheme), processScheme.IsObsolete);
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

            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            processSchemes = ignoreObsolete ?
                    dbcoll.Find(Query<WorkflowProcessScheme>.Where(pss => pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash)).ToList() :
                    dbcoll.Find(Query<WorkflowProcessScheme>.Where(pss => pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash && !pss.IsObsolete)).ToList();
            
            if (processSchemes.Count() < 1)
                throw new SchemeNotFoundException();

            if (processSchemes.Count() == 1)
            {
                var scheme = processSchemes.First();
                return new SchemeDefinition<XElement>(scheme.Id, XElement.Parse(scheme.Scheme), scheme.IsObsolete);
            }

            foreach (var processScheme in processSchemes.Where(processScheme => processScheme.DefiningParameters == definingParameters))
            {
                return new SchemeDefinition<XElement>(processScheme.Id, XElement.Parse(processScheme.Scheme), processScheme.IsObsolete);
            }

            throw new SchemeNotFoundException();
        }

        public void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = SerializeParameters(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            dbcoll.Update(
                Query<WorkflowProcessScheme>.Where(item => item.SchemeCode == schemeCode && item.DefiningParametersHash == definingParametersHash),
                Update<WorkflowProcessScheme>.Set(c => c.IsObsolete, true));
        }

        public void SetSchemeIsObsolete(string schemeCode)
        {
            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            dbcoll.Update(
                Query<WorkflowProcessScheme>.Where(item => item.SchemeCode == schemeCode),
                Update<WorkflowProcessScheme>.Set(c => c.IsObsolete, true));
        }

        public void SaveScheme(string schemeCode, Guid schemeId, XElement scheme, IDictionary<string, object> parameters)
        {
            var definingParameters = SerializeParameters(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            var dbcoll = Store.GetCollection<WorkflowProcessScheme>(MongoDBConstants.WorkflowProcessSchemeCollectionName);
            var oldSchemes =
                    dbcoll.Find(Query<WorkflowProcessScheme>.Where(
                        wps => wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == schemeCode && !wps.IsObsolete)).ToList();

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
            dbcoll.Insert(newProcessScheme);
        }

        public void SaveScheme(string schemaCode, string scheme)
        {
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme wfScheme = dbcoll.FindOne(Query<WorkflowScheme>.Where(c => c.Code == schemaCode));

            if (wfScheme == null)
            {
                wfScheme = new WorkflowScheme();
                wfScheme.Code = schemaCode;
                wfScheme.Scheme = scheme;
                dbcoll.Insert(wfScheme);
            }
            else
            {
                wfScheme.Scheme = scheme;
                dbcoll.Save(wfScheme);
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
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            WorkflowScheme scheme = dbcoll.FindOne(Query<WorkflowScheme>.Where(c => c.Code == code));

            if (scheme == null)
                throw new InvalidOperationException(string.Format("Scheme with Code={0} not found", code));

            return XElement.Parse(scheme.Scheme);
        }      

        #endregion

        #region IWorkflowGenerator
        protected IDictionary<string, string> TemplateTypeMapping = new Dictionary<string, string>();

        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
                throw new InvalidOperationException("Parameters not supported");

            var code = !TemplateTypeMapping.ContainsKey(schemeCode.ToLower()) ? schemeCode : TemplateTypeMapping[schemeCode.ToLower()];
            WorkflowScheme scheme = null;
            var dbcoll = Store.GetCollection<WorkflowScheme>(MongoDBConstants.WorkflowSchemeCollectionName);
            {
                scheme = dbcoll.FindOne(Query<WorkflowScheme>.Where(c=> c.Code == code));
            }

            if (scheme == null)
                throw new InvalidOperationException(string.Format("Scheme with Code={0} not found",code));

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

        
    }
}
