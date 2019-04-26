using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using Raven.Client.Document;
using Raven.Client.Linq;
using Raven.Imports.Newtonsoft.Json;

namespace OptimaJet.Workflow.RavenDB
{
    public class RavenDBProvider : IWorkflowProvider
    {
        private WorkflowRuntime _runtime;

        public RavenDBProvider(DocumentStore store)
        {
            Store = store;
            Store.Initialize();
        }

        public DocumentStore Store { get; set; }

        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
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

        public void InitializeProcess(ProcessInstance processInstance)
        {
            using (var session = Store.OpenSession())
            {
                var oldProcess = session.Load<WorkflowProcessInstance>(processInstance.ProcessId);
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
                    Persistence = new List<WorkflowProcessInstancePersistence>()
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
                    throw new ProcessNotFoundException(processInstance.ProcessId);

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
               processInstance.ProcessParameters.Where(ptp => ptp.Purpose == ParameterPurpose.Persistence)
                   .Select(ptp =>
                   {
                       if (ptp.Type == typeof(UnknownParameterType))
                           return new { Parameter = ptp, SerializedValue = (string)ptp.Value };
                       return new { Parameter = ptp, SerializedValue = ParametersSerializer.Serialize(ptp.Value, ptp.Type) };
                   })
                   .ToList();

            using (var session = Store.OpenSession())
            {
                var process = session.Load<WorkflowProcessInstance>(processInstance.ProcessId);
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
                }
                session.SaveChanges();
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
            using (var session = Store.OpenSession())
            {
                var instanceStatus = session.Load<WorkflowProcessInstanceStatus>(processInstance.ProcessId);
                if (instanceStatus == null)
                {
                    instanceStatus = new WorkflowProcessInstanceStatus
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
            var processId = processInstance.ProcessId;
            SetRunningStatus(processId);
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
            var session = Store.OpenSession();
            do
            {
                var prInst = session.Query<WorkflowProcessInstanceStatus>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(c => c.Status == 1).ToArray();
               
                if (prInst.Length == 0)
                    break;
                foreach (var item in prInst)
                {
                    item.Status = 2;
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

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            var identityId = paramIdentityId == null ? string.Empty : (string) paramIdentityId.Value;
            var impIdentityId = paramImpIdentityId == null ? identityId : (string) paramImpIdentityId.Value;

            using (var session = Store.OpenSession())
            {
                var inst = session.Load<WorkflowProcessInstance>(processInstance.ProcessId);
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
                }

                var history = new WorkflowProcessTransitionHistory
                {
                    ActorIdentityId = impIdentityId,
                    ExecutorIdentityId = identityId,
                    Id = Guid.NewGuid(),
                    IsFinalised = transition.To.IsFinal,
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
        
        private void SetRunningStatus(Guid processId)
        {
            using (var session = Store.OpenSession())
            {
                var instanceStatus = session.Load<WorkflowProcessInstanceStatus>(processId);
                if (instanceStatus == null)
                    throw new StatusNotDefinedException();

                if (instanceStatus.Status == ProcessStatus.Running.Id)
                    throw new ImpossibleToSetStatusException();

                instanceStatus.Lock = Guid.NewGuid();
                instanceStatus.Status = ProcessStatus.Running.Id;

                session.SaveChanges();
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

        private IEnumerable<ParameterDefinitionWithValue> GetPersistedProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count());

            List<WorkflowProcessInstancePersistence> persistedParameters;

            using (var session = Store.OpenSession())
            {
                var process = session.Load<WorkflowProcessInstance>(processId);
                if (process != null && process.Persistence != null)
                {
                    persistedParameters = process.Persistence;
                }
                else
                {
                    return parameters;
                    //persistedParameters = new List<WorkflowProcessInstancePersistence>();
                }
            }

            foreach (var persistedParameter in persistedParameters)
            {
                var parameterDefinition = persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.ParameterName);
                if (parameterDefinition == null)
                {
                    parameterDefinition = ParameterDefinition.Create(persistedParameter.ParameterName, typeof(UnknownParameterType), ParameterPurpose.Persistence);
                    parameters.Add(ParameterDefinition.Create(parameterDefinition, persistedParameter.Value));
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
            using (var session = Store.OpenSession())
            {
                var processInstance = session.Load<WorkflowProcessInstance>(processId);
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
            using (var session = Store.OpenSession())
            {
                var parameter = session.Query<WorkflowGlobalParameter>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).FirstOrDefault(p => p.Type == type && p.Name == name);

                if (parameter == null)
                {
                    parameter = new WorkflowGlobalParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Type = type
                    };

                    session.Store(parameter);
                }

                parameter.Value = JsonConvert.SerializeObject(value);

                session.SaveChanges();
            }
        }

        public T LoadGlobalParameter<T>(string type, string name)
        {
            using (var session = Store.OpenSession())
            {
                var parameter =
                    session.Query<WorkflowGlobalParameter>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).FirstOrDefault(p => p.Type == type && p.Name == name);

                if (parameter != null)
                {
                    return JsonConvert.DeserializeObject<T>(parameter.Value);
                }

                return default(T);
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
                    var parameters =
                        session.Query<WorkflowGlobalParameter>()
                            .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                            .Where(p => p.Type == type)
                            .Skip(skip)
                            .Take(session.Advanced.MaxNumberOfRequestsPerSession).ToList().Select(p => JsonConvert.DeserializeObject<T>(p.Value))
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
            WorkflowGlobalParameter[] wpths;

            do
            {
                using (var session = Store.OpenSession())
                {
                    wpths = string.IsNullOrEmpty(name)
                        ? session.Query<WorkflowGlobalParameter>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(p => p.Type == type).ToArray()
                        : session.Query<WorkflowGlobalParameter>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(p => p.Type == type && p.Name == name).ToArray();
                       

                    foreach (var wpth in wpths)
                        session.Delete(wpth);

                    session.SaveChanges();
                }
            } while (wpths.Length > 0);

        }

        public void DeleteProcess(Guid processId)
        {
            using (var session = Store.OpenSession())
            {
                var wpi = session.Load<WorkflowProcessInstance>(processId);
                if (wpi != null)
                    session.Delete(wpi);

                var wpis = session.Load<WorkflowProcessInstanceStatus>(processId);
                if (wpis != null)
                    session.Delete(wpis);

                session.SaveChanges();
            }

            WorkflowProcessTransitionHistory[] wpths;
            do
            {
                using (var session = Store.OpenSession())
                {
                    wpths = session.Query<WorkflowProcessTransitionHistory>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(c => c.ProcessId == processId).ToArray();

                    foreach (var wpth in wpths)
                        session.Delete(wpth);

                    session.SaveChanges();
                }
            } while (wpths.Length > 0);


            WorkflowProcessTimer[] wpts;
            do
            {
                using (var session = Store.OpenSession())
                {
                    wpts = session.Query<WorkflowProcessTimer>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(c => c.ProcessId == processId).ToArray();
                    foreach (var wpt in wpts)
                        session.Delete(wpt);

                    session.SaveChanges();
                }
            } while (wpts.Length > 0);
        }

        public void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            using (var session = Store.OpenSession())
            {
                var oldTimer =
                    session.Query<WorkflowProcessTimer>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).SingleOrDefault(wpt => wpt.ProcessId == processId && wpt.Name == name);

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
            List<WorkflowProcessTimer> timers;
            do
            {
                using (var session = Store.OpenSession())
                {
                    timers =
                        session.Query<WorkflowProcessTimer>()
                            .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                            .Where(wpt => wpt.ProcessId == processId && !wpt.Name.In(timersIgnoreList))
                            .ToList();
                    foreach (var timer in timers)
                        session.Delete(timer);

                    session.SaveChanges();
                }
            } while (timers.Count > 0);
        }

        public void ClearTimersIgnore()
        {
            WorkflowProcessTimer[] wpts;

            do
            {
                using (var session = Store.OpenSession())
                {
                    wpts = session.Query<WorkflowProcessTimer>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(t => t.Ignore).ToArray();
                    foreach (var wpth in wpts)
                        wpth.Ignore = false;

                    session.SaveChanges();
                }
            } while (wpts.Length > 0);
        }

        public void ClearTimerIgnore(Guid timerId)
        {
            using (var session = Store.OpenSession())
            {
                var timer = session.Load<WorkflowProcessTimer>(timerId);
                if (timer != null)
                {
                    timer.Ignore = false;
                    session.SaveChanges();
                }
            }
        }

        public void ClearTimer(Guid timerId)
        {
            using (var session = Store.OpenSession())
            {
                var timer = session.Load<WorkflowProcessTimer>(timerId);
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
                    session.Query<WorkflowProcessTimer>()
                        .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                        .Where(t => !t.Ignore)
                        .OrderBy(t => t.NextExecutionDateTime)
                        .FirstOrDefault();

                if (timer == null)
                    return null;

                return timer.NextExecutionDateTime;
            }
        }

        public List<TimerToExecute> GetTimersToExecute()
        {
            WorkflowProcessTimer[] wpts;
            var timers = new List<WorkflowProcessTimer>();
            var now = _runtime.RuntimeDateTimeNow;

            do
            {
                using (var session = Store.OpenSession())
                {
                    wpts = session.Query<WorkflowProcessTimer>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(t => !t.Ignore && t.NextExecutionDateTime <= now).ToArray();
                        // Queryable.Where(session.Query<WorkflowProcessTimer>(), t => !t.Ignore && t.NextExecutionDateTime <= now).ToArray();
                    foreach (var wpth in wpts)
                        wpth.Ignore = true;

                    timers.AddRange(wpts);
                    session.SaveChanges();
                }
            } while (wpts.Length > 0);

            return timers.Select(t => new TimerToExecute {Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id}).ToList();
        }

        public List<ProcessHistoryItem> GetProcessHistory(Guid processId)
        {
            var result = new List<ProcessHistoryItem>();
            var skip = 0;
            var session = Store.OpenSession();
            try
            {
                do
                {
                    var history =
                        session.Query<WorkflowProcessTransitionHistory>()
                            .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                            .Where(p => p.ProcessId == processId)
                            .Skip(skip)
                            .Take(session.Advanced.MaxNumberOfRequestsPerSession)
                            .ToList()
                            .Select(hi => new ProcessHistoryItem
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
                                TransitionTime = hi.TransitionTime,
                                TriggerName = hi.TriggerName
                            })
                            .ToList();

                    if (!history.Any())
                        break;

                    result.AddRange(history);

                    skip += session.Advanced.MaxNumberOfRequestsPerSession;
                } while (true);
            }
            finally
            {
                session.Dispose();
            }

            return result;
        }

        public IEnumerable<ProcessTimer> GetTimersForProcess(Guid processId)
        {
            var result = new List<ProcessTimer>();
            var skip = 0;
            var session = Store.OpenSession();
            try
            {
                do
                {
                    var history =
                        session.Query<WorkflowProcessTimer>()
                            .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                            .Where(p => p.ProcessId == processId)
                            .Skip(skip)
                            .Take(session.Advanced.MaxNumberOfRequestsPerSession)
                            .ToList()
                            .Select(hi => new ProcessTimer
                            {
                                Name = hi.Name,
                                NextExecutionDateTime = hi.NextExecutionDateTime
                            })
                            .ToList();

                    if (!history.Any())
                        break;

                    result.AddRange(history);

                    skip += session.Advanced.MaxNumberOfRequestsPerSession;
                } while (true);
            }
            finally
            {
                session.Dispose();
            }

            return result;
        }

        #endregion

        #region ISchemePersistenceProvider

        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            WorkflowProcessInstance processInstance;
            using (var session = Store.OpenSession())
            {
                processInstance = session.Load<WorkflowProcessInstance>(processId);
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
            using (var session = Store.OpenSession())
            {
                processScheme = session.Load<WorkflowProcessScheme>(schemeId);
            }

            if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                throw SchemeNotFoundException.Create(schemeId, SchemeLocation.WorkflowProcessScheme);

            return ConvertToSchemeDefinition(processScheme);
        }


        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, string definingParameters,
            Guid? rootSchemeId, bool ignoreObsolete)
        {
            IEnumerable<WorkflowProcessScheme> processSchemes;
            var hash = HashHelper.GenerateStringHash(definingParameters);

            using (var session = Store.OpenSession())
            {
                processSchemes = ignoreObsolete
                    ? session.Query<WorkflowProcessScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where( pss =>
                                pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash &&
                                pss.RootSchemeId == rootSchemeId && !pss.IsObsolete)
                        .ToList()
                    : session.Query<WorkflowProcessScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(pss =>
                                pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash &&
                                pss.RootSchemeId == rootSchemeId)
                        .ToList();
            }

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

            var session = Store.OpenSession();
            do
            {
                var oldSchemes =
                    session.Query<WorkflowProcessScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(wps =>
                            wps.DefiningParametersHash == definingParametersHash && (wps.SchemeCode == schemeCode || wps.RootSchemeCode == schemeCode) &&
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
                    session.Query<WorkflowProcessScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(wps =>
                            (wps.SchemeCode == schemeCode || wps.RootSchemeCode == schemeCode) &&
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

        public void SaveScheme(SchemeDefinition<XElement> scheme)
        {
            var definingParameters = scheme.DefiningParameters;
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (var session = Store.OpenSession())
            {
                var oldSchemes =
                    session.Query<WorkflowProcessScheme>()
                        .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                        .Where(wps => wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == scheme.SchemeCode && wps.IsObsolete == scheme.IsObsolete)
                        .ToList();

                if (oldSchemes.Any())
                {
                    if (oldSchemes.Any(oldScheme => oldScheme.DefiningParameters == definingParameters))
                    {
                        throw SchemeAlreadyExistsException.Create(scheme.SchemeCode, SchemeLocation.WorkflowProcessScheme, scheme.DefiningParameters);
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

                session.Store(newProcessScheme);
                session.SaveChanges();
            }
        }

        public void SaveScheme(string schemeCode,  bool canBeInlined, List<string> inlinedSchemes, string scheme)
        {
            using (var session = Store.OpenSession())
            {
                var wfscheme =
                    session.Query<WorkflowScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).FirstOrDefault(wps => wps.Code == schemeCode);
              
                if (wfscheme == null)
                {
                    wfscheme = new WorkflowScheme
                    {
                        Code = schemeCode,
                        Scheme = scheme,
                        CanBeInlined = canBeInlined,
                        InlinedSchemes = inlinedSchemes
                    };
                    session.Store(wfscheme);
                }
                else
                {
                    wfscheme.CanBeInlined = canBeInlined;
                    wfscheme.InlinedSchemes = inlinedSchemes;
                    wfscheme.Scheme = scheme;
                }

                session.SaveChanges();
            }
        }

        public List<string> GetInlinedSchemeCodes()
        {
            using (var session = Store.OpenSession())
            {
                var codes =
                    session.Query<WorkflowScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(sch => sch.CanBeInlined).Select(sch => sch.Code).ToList();
                return codes;
            }
        }

        public List<string> GetRelatedByInliningSchemeCodes(string schemeCode)
        {
            using (var session = Store.OpenSession())
            {
                var codes =
                    session.Query<WorkflowScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Where(sch => sch.InlinedSchemes.ContainsAny(new[] {schemeCode}))
                        .Select(sch => sch.Code).ToList();
                return codes;
            }
        }

        public XElement GetScheme(string code)
        {
            using (var session = Store.OpenSession())
            {
                var wfscheme =
                    session.Query<WorkflowScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).FirstOrDefault(wps => wps.Code == code);

                if (wfscheme == null || string.IsNullOrEmpty(wfscheme.Scheme))
                    throw SchemeNotFoundException.Create(code, SchemeLocation.WorkflowScheme);
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
            WorkflowScheme scheme;
            using (var session = Store.OpenSession())
            {
                scheme = session.Query<WorkflowScheme>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).FirstOrDefault(wps => wps.Code == code);
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
    }
}