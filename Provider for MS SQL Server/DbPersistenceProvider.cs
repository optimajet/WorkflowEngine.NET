using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using ServiceStack.Text;

namespace OptimaJet.Workflow.DbPersistence
{
    public sealed class DbPersistenceProvider : DbProvider, IPersistenceProvider
    {
        private Core.Runtime.WorkflowRuntime _runtime;

        public DbPersistenceProvider(string connectionString) : base(connectionString)
        {
        }


        public void Init(Core.Runtime.WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public void InitializeProcess(ProcessInstance processInstance)
        {
            using (var scope = PredefinedTransactionScopes.ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var oldProcess = context.WorkflowProcessInstances.SingleOrDefault(wpi => wpi.Id == processInstance.ProcessId);
                    if (oldProcess != null)
                        throw new ProcessAlredyExistsException();
                    var newProcess = new WorkflowProcessInstance
                                         {
                                             Id = processInstance.ProcessId,
                                             SchemeId = processInstance.SchemeId,
                                             ActivityName = processInstance.ProcessScheme.InitialActivity.Name,
                                             StateName = processInstance.ProcessScheme.InitialActivity.State
                                         };
                    context.WorkflowProcessInstances.InsertOnSubmit(newProcess);
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }


        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            BindProcessToNewScheme(processInstance,false);
        }

        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            using (var scope = PredefinedTransactionScopes.ReadCommittedSupressedScope)
            {

                using (var context = CreateContext())
                {
                    var oldProcess =
                        context.WorkflowProcessInstances.SingleOrDefault(wpi => wpi.Id == processInstance.ProcessId);
                   if (oldProcess == null)
                        throw new ProcessNotFoundException();
                    oldProcess.SchemeId = processInstance.SchemeId;
                    if (resetIsDeterminingParametersChanged)
                        oldProcess.IsDeterminingParametersChanged = false;
                    context.SubmitChanges();
                }
                scope.Complete();
            }
        }

        public void RegisterTimer(Guid processId, string name, DateTime nextExecutionDateTime, bool notOverrideIfExists)
        {
            using (var context = CreateContext())
            {
                var oldTimer =
                    context.WorkflowProcessTimers.SingleOrDefault(wpt => wpt.ProcessId == processId && wpt.Name == name);

                if (oldTimer == null)
                {
                    oldTimer = new WorkflowProcessTimer()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        NextExecutionDateTime = nextExecutionDateTime,
                        ProcessId = processId
                    };

                    oldTimer.Ignore = false;

                    context.WorkflowProcessTimers.InsertOnSubmit(oldTimer);
                }

                if (!notOverrideIfExists)
                {
                    oldTimer.NextExecutionDateTime = nextExecutionDateTime;
                }

                context.SubmitChanges();
            }
        }

        public void ClearTimers(Guid processId, List<string> timersIgnoreList)
        {
            using (var context = CreateContext())
            {
                var timers = context.WorkflowProcessTimers.Where(wpt => wpt.ProcessId == processId && !timersIgnoreList.Contains(wpt.Name)).ToList();

                context.WorkflowProcessTimers.DeleteAllOnSubmit(timers);
                
                context.SubmitChanges();
            }
        }

        public void ClearTimersIgnore()
        {
            using (var context = CreateContext())
            {
                var timers =
                    context.WorkflowProcessTimers.Where(
                        wpt => wpt.Ignore).ToList();

                foreach (var timer in timers)
                {
                    timer.Ignore = false;
                }

                context.SubmitChanges();
            }

        }

        public void ClearTimer(Guid timerId)
        {
            using (var context = CreateContext())
            {
                var timer = context.WorkflowProcessTimers.FirstOrDefault(t => t.Id == timerId);

                if (timer != null)
                {
                    context.WorkflowProcessTimers.DeleteOnSubmit(timer);

                    context.SubmitChanges();
                }

            }
        }

        public DateTime? GetCloseExecutionDateTime()
        {
            using (var context = CreateContext())
            {
                var timer =
                    context.WorkflowProcessTimers.Where(t=>!t.Ignore).OrderBy(wpt => wpt.NextExecutionDateTime).FirstOrDefault();

                if (timer == null)
                    return null;

                return timer.NextExecutionDateTime;
            }
        }

        public List<TimerToExecute> GetTimersToExecute()
        {
            using (var context = CreateContext())
            {
                var now = _runtime.RuntimeDateTimeNow;
                var timers =
                    context.WorkflowProcessTimers.Where(t => !t.Ignore && t.NextExecutionDateTime <= now).ToList();
                foreach (var timer in timers)
                {
                    timer.Ignore = true;
                }
                context.SubmitChanges();
                return timers.Select(t => new TimerToExecute() {Name = t.Name, ProcessId = t.ProcessId, TimerId = t.Id}).ToList();
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
                    .Select(ptp => new {Parameter = ptp, SerializedValue = _runtime.SerializeParameter(ptp.Value,ptp.Type)})
                    .ToList();
           
            using (var scope = PredefinedTransactionScopes.ReadUncommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                   var persistedParameters =
                      context.WorkflowProcessInstancePersistences.Where(
                          wpip =>
                          wpip.ProcessId == processInstance.ProcessId).ToList();

                    foreach (var parameterDefinitionWithValue in parametersToPersistList)
                    {
                        WorkflowProcessInstancePersistence persistence =
                            persistedParameters.SingleOrDefault(
                                pp => pp.ParameterName == parameterDefinitionWithValue.Parameter.Name);
                        {
                            if (persistence == null)
                            {
                                if (parameterDefinitionWithValue.SerializedValue != null)
                                {
                                    persistence = new WorkflowProcessInstancePersistence()
                                                      {
                                                          Id = Guid.NewGuid(),
                                                          ParameterName = parameterDefinitionWithValue.Parameter.Name,
                                                          ProcessId = processInstance.ProcessId,
                                                          Value = parameterDefinitionWithValue.SerializedValue
                                                      };
                                    context.WorkflowProcessInstancePersistences.InsertOnSubmit(persistence);
                                }
                            }
                            else 
                            {
                                if (parameterDefinitionWithValue.SerializedValue != null)
                                 persistence.Value = parameterDefinitionWithValue.SerializedValue;
                                else
                                    context.WorkflowProcessInstancePersistences.DeleteOnSubmit(persistence);
                            }
                        }
                    }

                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        public void SetWorkflowIniialized(ProcessInstance processInstance)
        {
            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var instanceStatus = context.WorkflowProcessInstanceStatus.SingleOrDefault(wpis => wpis.Id == processInstance.ProcessId);
                    if (instanceStatus == null)
                    {
                        instanceStatus = new WorkflowProcessInstanceStatus()
                                        {
                                            Id = processInstance.ProcessId,
                                            Lock = Guid.NewGuid(),
                                            Status = ProcessStatus.Initialized.Id
                                        };

                        context.WorkflowProcessInstanceStatus.InsertOnSubmit(instanceStatus);

                    }
                    else
                    {
                        instanceStatus.Status = ProcessStatus.Initialized.Id;
                    }

                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        public void SetWorkflowIdled(ProcessInstance processInstance)
        {
            SetCustomStatus(processInstance.ProcessId,ProcessStatus.Idled);
        }

        public void SetWorkflowRunning(ProcessInstance processInstance)
        {
            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var instanceStatus = context.WorkflowProcessInstanceStatus.SingleOrDefault(wpis => wpis.Id == processInstance.ProcessId);
                    if (instanceStatus == null)
                        throw new StatusNotDefinedException();
                    
                    if (instanceStatus.Status == ProcessStatus.Running.Id)
                        throw new ImpossibleToSetStatusException();

                    instanceStatus.Lock = Guid.NewGuid();
                    instanceStatus.Status = ProcessStatus.Running.Id;

                    try
                    {
                        context.SubmitChanges();
                    }
                    catch(Exception ex)
                    {
                        throw new ImpossibleToSetStatusException(ex.Message, ex);
                    }
                }

                scope.Complete();
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
            using (var context = CreateContext())
            {
                context.spWorkflowProcessResetRunningStatus();
            }
        }

        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            var paramIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterIdentityId.Name);
            var paramImpIdentityId = processInstance.GetParameter(DefaultDefinitions.ParameterImpersonatedIdentityId.Name);

            string identityId = paramIdentityId == null || paramIdentityId.Value == null
                ? string.Empty
                : paramIdentityId.Value.ToString();
            string impIdentityId = paramImpIdentityId == null || paramImpIdentityId.Value == null
                ? identityId
                : paramImpIdentityId.Value.ToString();

            using (var context = CreateContext())
            {
                WorkflowProcessInstance inst = context.WorkflowProcessInstances.FirstOrDefault(c => c.Id == processInstance.ProcessId);
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
                }

                var history = new WorkflowProcessTransitionHistory()
                {
                    ActorIdentityId = impIdentityId,
                    ExecutorIdentityId = identityId,
                    Id = Guid.NewGuid(),
                    IsFinalised = false,
                    //TODO Зачем на м финализед тут????
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


                context.WorkflowProcessTransitionHistories.InsertOnSubmit(history);

                context.SubmitChanges();
            }

        }

        public bool IsProcessExists(Guid processId)
        {
            using (var context = CreateContext())
            {
                return context.WorkflowProcessInstances.Count(wpi => wpi.Id == processId) > 0;
            }
        }

        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            using (var context = CreateContext())
            {
                var instance = context.WorkflowProcessInstanceStatus.FirstOrDefault(wpis => wpis.Id == processId);
                if (instance == null)
                    return ProcessStatus.NotFound;
                var status = ProcessStatus.All.SingleOrDefault(ins => ins.Id == instance.Status);
                if (status == null)
                    return ProcessStatus.Unknown;
                return status;
            }
        }


        private void SetCustomStatus (Guid processId, ProcessStatus status)
        {
            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var instanceStatus = context.WorkflowProcessInstanceStatus.SingleOrDefault(wpis => wpis.Id == processId);
                    if (instanceStatus == null)
                        throw new StatusNotDefinedException();
                    instanceStatus.Status = status.Id;

                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        private IEnumerable<ParameterDefinitionWithValue> GetProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            var parameters = new List<ParameterDefinitionWithValue>(processDefinition.Parameters.Count());
            parameters.AddRange(GetPersistedProcessParameters(processId,processDefinition));
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

        private IEnumerable<ParameterDefinitionWithValue> GetPersistedProcessParameters (Guid processId, ProcessDefinition processDefinition)
        {
            var persistenceParameters = processDefinition.PersistenceParameters.ToList();
            var parameters = new List<ParameterDefinitionWithValue>(persistenceParameters.Count());
            
            List<WorkflowProcessInstancePersistence> persistedParameters;
            using (PredefinedTransactionScopes.ReadUncommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    persistedParameters =
                        context.WorkflowProcessInstancePersistences.Where(
                            wpip =>
                            wpip.ProcessId == processId).ToList();
                }
            }

            foreach (var persistedParameter in persistedParameters)
            {
                var parameterDefinition = persistenceParameters.FirstOrDefault(p => p.Name == persistedParameter.ParameterName);
                if (parameterDefinition == null)
                    parameterDefinition = ParameterDefinition.Create(persistedParameter.ParameterName,"System.String",ParameterPurpose.Persistence.ToString(),null);

                parameters.Add(ParameterDefinition.Create(parameterDefinition, _runtime.DeserializeParameter(persistedParameter.Value, parameterDefinition.Type)));
                
            }

            return parameters;
        }

       

        private WorkflowProcessInstance GetProcessInstance (Guid processId)
        {
            using (PredefinedTransactionScopes.ReadCommittedSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var processInstance = context.WorkflowProcessInstances.SingleOrDefault(wpi => wpi.Id == processId);
                    if (processInstance == null)
                        throw new ProcessNotFoundException();
                    return processInstance;
                }
            }
        }

        public void DeleteProcess(Guid processId)
        {
            using (var context = CreateContext())
            {
                var wpi = context.WorkflowProcessInstances.Where(c=> c.Id == processId).FirstOrDefault();
                if (wpi != null)
                    context.WorkflowProcessInstances.DeleteOnSubmit(wpi);

                var wpis = context.WorkflowProcessInstanceStatus.Where(c => c.Id == processId).FirstOrDefault();
                if (wpis != null)
                    context.WorkflowProcessInstanceStatus.DeleteOnSubmit(wpis);

                var wpths = context.WorkflowProcessTransitionHistories.Where(c => c.ProcessId == processId).ToArray();
                context.WorkflowProcessTransitionHistories.DeleteAllOnSubmit(wpths);

                var wpts = context.WorkflowProcessTimers.Where(c => c.ProcessId == processId).ToArray();
                context.WorkflowProcessTimers.DeleteAllOnSubmit(wpts);
                
                context.SubmitChanges();
            }
        }


        public void DeleteProcess(Guid[] processIds)
        {
            foreach (var p in processIds)
                DeleteProcess(p);
        }

        public void SaveGlobalParameter<T>(string type, string name, T value)
        {
            using (var context = CreateContext())
            {
                var parameter = context.WorkflowGlobalParameters.FirstOrDefault(p => p.Type == type && p.Name == name);

                if (parameter == null)
                {
                    parameter = new WorkflowGlobalParameter
                    {
                        Id = Guid.NewGuid(),
                        Type = type,
                        Name = name
                    };

                    context.WorkflowGlobalParameters.InsertOnSubmit(parameter);

                }

                parameter.Value = JsonSerializer.SerializeToString(value);

                context.SubmitChanges();
            }
        }

        public T LoadGlobalParameter<T>(string type, string name) 
        {
            using (var context = CreateContext())
            {
                var parameter = context.WorkflowGlobalParameters.FirstOrDefault(p => p.Type == type && p.Name == name);

                if (parameter == null)
                    return default(T);

                return JsonSerializer.DeserializeFromString<T>(parameter.Value);
            }
        }

        public List<T> LoadGlobalParameters<T>(string type)
        {
            using (var context = CreateContext())
            {
                var parameters = context.WorkflowGlobalParameters.Where(p => p.Type == type).ToList();
                return parameters.Select(p=>JsonSerializer.DeserializeFromString<T>(p.Value)).ToList();
            }
        }

        public void DeleteGlobalParameters(string type, string name = null)
        {
            using (var context = CreateContext())
            {
                var parameters = context.WorkflowGlobalParameters.Where(p => p.Type == type && (string.IsNullOrEmpty(name) || p.Name == name)).ToList();
                context.WorkflowGlobalParameters.DeleteAllOnSubmit(parameters);
                context.SubmitChanges();
            }
        }
    }
}
