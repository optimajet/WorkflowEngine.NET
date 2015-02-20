using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using Raven.Client.Document;
using ServiceStack.Text;

namespace OptimaJet.Workflow.RavenDB
{
    public class RavenDBProvider : IPersistenceProvider, ISchemePersistenceProvider<XElement>, IWorkflowGenerator<XElement>
    {
        public DocumentStore Store { get; set; }

        private Core.Runtime.WorkflowRuntime _runtime;
        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public RavenDBProvider(DocumentStore store)
        {
            Store = store;
            Store.Initialize();
        }

        #region IPersistenceProvider
        public void InitializeProcess(ProcessInstance processInstance)
        {
            using(var session = Store.OpenSession())
            {
                var oldProcess = session.Load<WorkflowProcessInstance>(processInstance.ProcessId);
                if (oldProcess != null)
                {
                    throw new ProcessAlredyExistsException();
                }
                var newProcess = new WorkflowProcessInstance {
                                         Id = processInstance.ProcessId,
                                         SchemeId = processInstance.SchemeId,
                                         ActivityName = processInstance.ProcessScheme.InitialActivity.Name,
                                         StateName = processInstance.ProcessScheme.InitialActivity.State
                                     };
                session.Store(newProcess);
                session.SaveChanges();
            }
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            BindProcessToNewScheme(processInstance, false);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            using (var session = Store.OpenSession())
            {
                var oldProcess = session.Load<WorkflowProcessInstance>(processInstance.ProcessId);
                if (oldProcess == null)
                    throw new ProcessNotFoundException();

                oldProcess.SchemeId = processInstance.SchemeId;
                if (resetIsDeterminingParametersChanged)
                    oldProcess.IsDeterminingParametersChanged = false;
                session.Store(oldProcess);
                session.SaveChanges();
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
              processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence).Select(ptp => new { Parameter = ptp, SerializedValue = _runtime.SerializeParameter(ptp.Value,ptp.Type) })
                  .ToList();
            var persistenceParameters = processInstance.ProcessScheme.PersistenceParameters.ToList();

            using (var session = Store.OpenSession())
            {
                var process = session.Load<WorkflowProcessInstance>(processInstance.ProcessId);
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
                }
                session.SaveChanges();
            }
        }

        public void SetWorkflowIniialized(ProcessInstance processInstance)
        {
            using (var session = Store.OpenSession())
            {
                var instanceStatus = session.Load<WorkflowProcessInstanceStatus>(processInstance.ProcessId);
                if (instanceStatus == null)
                {
                    instanceStatus = new WorkflowProcessInstanceStatus()
                    {
                        Id = processInstance.ProcessId,
                        Lock = Guid.NewGuid(),
                        Status = ProcessStatus.Initialized.Id
                    };

                    session.Store(instanceStatus);

                }
                else
                {
                    instanceStatus.Status = ProcessStatus.Initialized.Id;
                }

                session.SaveChanges();
            }
        }

        public void SetWorkflowIdled(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId, ProcessStatus.Idled);
        }

        public void SetWorkflowRunning(ProcessInstance processInstance)
        {
            using (var session = Store.OpenSession())
            {
                var instanceStatus = session.Load<WorkflowProcessInstanceStatus>(processInstance.ProcessId);
                if (instanceStatus == null)
                    throw new StatusNotDefinedException();
                
                if (instanceStatus.Status == ProcessStatus.Running.Id)
                    throw new ImpossibleToSetStatusException();

                instanceStatus.Lock = Guid.NewGuid();
                instanceStatus.Status = ProcessStatus.Running.Id;

                session.SaveChanges();
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
            var session = Store.OpenSession();
            do
            {
                var prInst = session.Query<WorkflowProcessInstanceStatus>().Where(c=> c.Status == 1).ToArray();
                
                if(prInst.Length == 0)
                    break;
                foreach (var item in prInst)
                {
                    item.Status = 2;
                }

                session.SaveChanges();

                if(session.Advanced.NumberOfRequests >= session.Advanced.MaxNumberOfRequestsPerSession - 2)
                {
                    session.Dispose();
                    session = Store.OpenSession();
                }
            } while (true);

            session.Dispose();
        }

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            string identityId = paramIdentityId == null ? string.Empty : (string)paramIdentityId.Value;
            string impIdentityId = paramImpIdentityId == null ? identityId : (string)paramImpIdentityId.Value;

            using (var session = Store.OpenSession())
            {
                WorkflowProcessInstance inst = session.Load<WorkflowProcessInstance>(processInstance.ProcessId);
                if(inst != null)
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

                session.Store(history);
                session.SaveChanges();
            }
        }

        public bool IsProcessExists(Guid processId)
        {
            using (var session = Store.OpenSession())
            {
                return session.Load<WorkflowProcessInstance>(processId) != null;
            }
        }

        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            using (var session = Store.OpenSession())
            {
                var instance = session.Load<WorkflowProcessInstanceStatus>(processId);
                if (instance == null)
                    return ProcessStatus.NotFound;
                var status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status);
                if (status == null)
                    return ProcessStatus.Unknown;
                return status;
            }
        }


        private void SetCustomStatus(Guid processId, ProcessStatus status)
        {
            using (var session = Store.OpenSession())
            {
                var instanceStatus = session.Load<WorkflowProcessInstanceStatus>(processId);
                if (instanceStatus == null)
                    throw new StatusNotDefinedException();
                instanceStatus.Status = status.Id;

                session.SaveChanges();
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

            using (var session = Store.OpenSession())
            {
                var process = session.Load<WorkflowProcessInstance>(processId);
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
            using (var session = Store.OpenSession())
            {
                var processInstance = session.Load<WorkflowProcessInstance>(processId);
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

        public void SaveGlobalParameter<T>(string type, string name, T value)
        {
            using (var session = Store.OpenSession())
            {
                var parameter = session.Query<WorkflowGlobalParameter>().FirstOrDefault(p => p.Type == type && p.Name == name);

                if (parameter == null)
                {
                    parameter = new WorkflowGlobalParameter()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Type = type
                    };

                    session.Store(parameter);
                }

                parameter.Value = JsonSerializer.SerializeToString(value);

                session.SaveChanges();
            }
        }

        public T LoadGlobalParameter<T>(string type, string name)
        {
            using (var session = Store.OpenSession())
            {
                var parameter =
                    session.Query<WorkflowGlobalParameter>().FirstOrDefault(p => p.Type == type && p.Name == name);

                if (parameter != null)
                {
                    return JsonSerializer.DeserializeFromString<T>(parameter.Value);
                }

                return default (T);
            }
        }

        public List<T> LoadGlobalParameters<T>(string type)
        {
            var result = new List<T>();
            var skip = 0;
            var session = Store.OpenSession();
            try
            {
                do
                {
                    var parameters = session.Query<WorkflowGlobalParameter>()
                        .Where(p => p.Type == type).Skip(skip).Take(session.Advanced.MaxNumberOfRequestsPerSession).ToList()
                        .Select(p => JsonSerializer.DeserializeFromString<T>(p.Value))
                        .ToList();

                    if (!parameters.Any())
                        break;

                    result.AddRange(parameters);

                    skip += session.Advanced.MaxNumberOfRequestsPerSession;

                } while (true);
            }
            finally
            {
                session.Dispose();
            }

            return result;
        }

        public void DeleteGlobalParameters(string type, string name = null)
        {

            var session = Store.OpenSession();
            try
            {

                do
                {
                    var pInst =
                        session.Query<WorkflowGlobalParameter>()
                            .Where(p => p.Type == type && (string.IsNullOrEmpty(name) || p.Name == name))
                            .Take(session.Advanced.MaxNumberOfRequestsPerSession - 1)
                            .ToArray();

                    if (pInst.Length == 0)
                        break;

                    foreach (var item in pInst)
                    {
                        session.Delete(item);
                    }

                    session.SaveChanges();
                    session.Dispose();
                    session = Store.OpenSession();

                } while (true);
            }
            finally
            {
                session.Dispose();
            }

        }

        public void DeleteProcess(Guid processId)
        {
            using (var session = Store.OpenSession())
            {
                var wpi = session.Load<WorkflowProcessInstance>(processId);
                if (wpi != null)
                    session.Delete<WorkflowProcessInstance>(wpi);

                var wpis = session.Load<WorkflowProcessInstanceStatus>(processId);
                if (wpis != null)
                    session.Delete<WorkflowProcessInstanceStatus>(wpis);

                session.SaveChanges();
            }

            WorkflowProcessTransitionHistory[] wpths = null;
            do
            {
                using (var session = Store.OpenSession())
                {
                    wpths = session.Query<WorkflowProcessTransitionHistory>().Where(c => c.ProcessId == processId).ToArray();
                    foreach (var wpth in wpths)
                        session.Delete<WorkflowProcessTransitionHistory>(wpth);

                    session.SaveChanges();
                }
            } while (wpths.Length > 0);


            WorkflowProcessTimer[] wpts = null;
            do
            {
                using (var session = Store.OpenSession())
                {
                    wpts = session.Query<WorkflowProcessTimer>().Where(c => c.ProcessId == processId).ToArray();
                    foreach (var wpt in wpts)
                        session.Delete<WorkflowProcessTimer>(wpt);

                    session.SaveChanges();
                }
            } while (wpts.Length > 0);
        }

        public void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            using (var session = Store.OpenSession())
            {
                var oldTimer =
                    session.Query<WorkflowProcessTimer>().SingleOrDefault(wpt => wpt.ProcessId == processId && wpt.Name == name);

                if (oldTimer == null)
                {
                    oldTimer = new WorkflowProcessTimer
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        NextExecutionDateTime = nextExecutionDateTime,
                        ProcessId = processId,
                        Ignore = false
                    };


                    session.Store(oldTimer);
                }

                if (!notOverrideIfExists)
                {
                    oldTimer.NextExecutionDateTime = nextExecutionDateTime;
                }

                session.SaveChanges();
            }
        }

        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            using (var session = Store.OpenSession())
            {
                var timers = session.Query<WorkflowProcessTimer>().Where(wpt => wpt.ProcessId == processId).ToList();
                timers.Where(wpt => !timersIgnoreList.Contains(wpt.Name)).ToList().ForEach((t) => session.Delete(t));                    
                session.SaveChanges();
            }
        }

        public void ClearTimersIgnore()
        {
            WorkflowProcessTimer[] wpts = null;

            do
            {
                using (var session = Store.OpenSession())
                {
                    wpts = session.Query<WorkflowProcessTimer>().Where(t => t.Ignore).ToArray();
                    foreach (var wpth in wpts)
                        wpth.Ignore = false;

                    session.SaveChanges();
                }
            } while (wpts.Length > 0);
        }

        public void ClearTimer(Guid timerId)
        {
            using (var session = Store.OpenSession())
            {
                var timer = session.Load <WorkflowProcessTimer>(timerId);
                if (timer != null)
                {
                    session.Delete(timer);
                    session.SaveChanges();
                }
            }
        }

        public DateTime? GetCloseExecutionDateTime()
        {
            using (var session = Store.OpenSession())
            {
                var timer =
                    session.Query<WorkflowProcessTimer>().Where(t => !t.Ignore).OrderBy(wpt => wpt.NextExecutionDateTime).FirstOrDefault();

                if (timer == null)
                    return null;

                return timer.NextExecutionDateTime;
            }
        }

        public List<TimerToExecute> GetTimersToExecute()
        {
            WorkflowProcessTimer[] wpts = null;
            List<WorkflowProcessTimer> timers = new List<WorkflowProcessTimer>();
            var now = _runtime.RuntimeDateTimeNow;

            do
            {
                using (var session = Store.OpenSession())
                {
                    wpts = session.Query<WorkflowProcessTimer>().Where(t => !t.Ignore && t.NextExecutionDateTime <= now).ToArray();
                    foreach (var wpth in wpts)
                        wpth.Ignore = true;

                    timers.AddRange(wpts);
                    session.SaveChanges();
                }
            } while (wpts.Length > 0);

            return timers.Select(t => new TimerToExecute() { Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id }).ToList();
        }

        #endregion 

        #region ISchemePersistenceProvider
        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            WorkflowProcessInstance processInstance = new WorkflowProcessInstance();
            using (var session = Store.OpenSession())
            {
                processInstance = session.Load<WorkflowProcessInstance>(processId);
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
            using (var session = Store.OpenSession())
            {
                processScheme = session.Load<WorkflowProcessScheme>(schemeId);
            }

            if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                throw new SchemeNotFoundException();

            return new SchemeDefinition<XElement>(schemeId, processScheme.SchemeCode, XElement.Parse(processScheme.Scheme), processScheme.IsObsolete);
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

            using (var session = Store.OpenSession())
            {
                processSchemes = ignoreObsolete ?
                    session.Query<WorkflowProcessScheme>().Where(pss => pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash).ToList() :
                    session.Query<WorkflowProcessScheme>().Where(pss => pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash && !pss.IsObsolete).ToList();
            }

            if (processSchemes.Count() < 1)
                throw new SchemeNotFoundException();

            if (processSchemes.Count() == 1)
            {
                var scheme = processSchemes.First();
                return new SchemeDefinition<XElement>(scheme.Id, scheme.SchemeCode, XElement.Parse(scheme.Scheme), scheme.IsObsolete);
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

            var session = Store.OpenSession();
            do
            {
                var oldSchemes =
                         session.Query<WorkflowProcessScheme>().Where(
                             wps =>
                                 wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == schemeCode &&
                                 !wps.IsObsolete).ToList();

                if (oldSchemes.Count == 0)
                    break;

                foreach (var scheme in oldSchemes)
                {
                    scheme.IsObsolete = true;
                }

                session.SaveChanges();

                if (session.Advanced.NumberOfRequests >= session.Advanced.MaxNumberOfRequestsPerSession - 2)
                {
                    session.Dispose();
                    session = Store.OpenSession();
                }
            } while (true);

            session.Dispose();
        }

        public void SetSchemeIsObsolete(string schemeCode)
        {
            var session = Store.OpenSession();
            do
            {
                var oldSchemes =
                         session.Query<WorkflowProcessScheme>().Where(
                             wps =>
                                  wps.SchemeCode == schemeCode &&
                                 !wps.IsObsolete).ToList();

                if (oldSchemes.Count == 0)
                    break;

                foreach (var scheme in oldSchemes)
                {
                    scheme.IsObsolete = true;
                }

                session.SaveChanges();

                if (session.Advanced.NumberOfRequests >= session.Advanced.MaxNumberOfRequestsPerSession - 2)
                {
                    session.Dispose();
                    session = Store.OpenSession();
                }
            } while (true);

            session.Dispose();
        }

        public void SaveScheme(string schemeCode, Guid schemeId, XElement scheme, IDictionary<string, object> parameters)
        {
            var definingParameters = SerializeParameters(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (var session = Store.OpenSession())
            {
                var oldSchemes =
                    session.Query<WorkflowProcessScheme>().Where(
                        wps => wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == schemeCode && !wps.IsObsolete).ToList();

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
                session.Store(newProcessScheme);
                session.SaveChanges();
            }
        }

        public void SaveScheme(string schemeCode, string scheme)
        {
            using (var session = Store.OpenSession())
            {
                var wfscheme =
                    session.Query<WorkflowScheme>().FirstOrDefault(wps => wps.Code == schemeCode);

                if (wfscheme == null)
                {
                    wfscheme = new WorkflowScheme()
                    {
                        Code = schemeCode,
                        Scheme = scheme
                    };
                    session.Store(wfscheme);
                }
                else
                {
                    wfscheme.Scheme = scheme;
                }

                session.SaveChanges();
            }
        }
        public XElement GetScheme(string code)
        {
            using (var session = Store.OpenSession())
            {
                var wfscheme =
                    session.Query<WorkflowScheme>().FirstOrDefault(wps => wps.Code == code);

                if (wfscheme == null || string.IsNullOrEmpty(wfscheme.Scheme))
                    throw new SchemeNotFoundException();

                return XElement.Parse(wfscheme.Scheme);
            }
        }

        #endregion

        #region IWorkflowGenerator
        protected IDictionary<string, string> TemplateTypeMapping = new Dictionary<string, string>();

        public XElement Generate(string processName, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
                throw new InvalidOperationException("Parameters not supported");

            var code = !TemplateTypeMapping.ContainsKey(processName.ToLower()) ? processName : TemplateTypeMapping[processName.ToLower()];
            WorkflowScheme scheme = null;
            using (var session = Store.OpenSession())
            {
                scheme = session.Load<WorkflowScheme>(code);
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

       
    }
}
