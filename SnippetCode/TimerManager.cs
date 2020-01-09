using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent a timer to register in persistence store
    /// </summary>
    public class TimerToRegister
    {
        /// <summary>
        /// Timer name <see cref="TimerDefinition.Name"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the process which owned the timer
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Execution DateTime of timer
        /// </summary>
        public DateTime ExecutionDateTime { get; set; }
    }

    /// <summary>
    /// Represent a timer information
    /// </summary>
    public class TimerToExecute
    {
        /// <summary>
        /// Timer id
        /// </summary>
        public Guid TimerId { get; set; }
        /// <summary>
        /// Timer name <see cref="TimerDefinition.Name"/>
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Id of the process which owned the timer
        /// </summary>
        public Guid ProcessId { get; set; }
    }

    /// <summary>
    /// Default timer manager <see cref="ITimerManager"/>
    /// </summary>
    public class TimerManager : ITimerManager
    {
        /// <summary>
        /// Value of Unspecified Timer which indicates that the timer transition will be executed immediately
        /// </summary>
        public string ImmediateTimerValue { get; } = "0";

        /// <summary>
        /// Value of Unspecified Timer which indicates that the timer transition will be never executed
        /// </summary>
        public string InfinityTimerValue { get; } = "-1";

        //private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly SemaphoreSlim _startStopSemaphore = new SemaphoreSlim(1);

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Wait timeout for start/stop operations in milliseconds. Default value is 1000.
        /// </summary>
        public int DefaultWaitTimeout { get; set; }

        private WorkflowRuntime _runtime;

        private Timer _timer;

        private bool _stopped = true;

        /// <summary>
        /// Timer manager constructor
        /// </summary>
        public TimerManager()
        {
            DefaultWaitTimeout = 1000;
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Raises when the timer value must be obtained 
        /// </summary>
        public event EventHandler<NeedTimerValueEventArgs> NeedTimerValue;

        /// <summary>
        /// Sends request for timer value for all timer transitions that are outgoing from the CurrentActivity if timer value is equal 0 or -1
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        public void RequestTimerValue(ProcessInstance processInstance, ActivityDefinition activity = null)
        {
            var needTimerValueHandler = NeedTimerValue;
            if (needTimerValueHandler == null)
                return;

            var notDefinedTimerTransitions =
                processInstance.ProcessScheme.GetTimerTransitionForActivity(activity ?? processInstance.CurrentActivity, ForkTransitionSearchType.Both)
                    .Where(
                        t =>
                            (t.Trigger.Timer.Value.Equals(ImmediateTimerValue) || t.Trigger.Timer.Value.Equals(InfinityTimerValue)) &&
                            !processInstance.IsParameterExisting(GetTimerValueParameterName(t.Trigger.Timer))).ToList();

            if (!notDefinedTimerTransitions.Any())
                return;

            var eventArgs = new NeedTimerValueEventArgs(processInstance, notDefinedTimerTransitions);

            needTimerValueHandler(this, eventArgs);

            foreach (var request in eventArgs.TimerValueRequests)
            {
                if (request.NewValue.HasValue)
                    processInstance.SetParameter(GetTimerValueParameterName(request.Definition), request.NewValue.Value, ParameterPurpose.Persistence);
            }
        }

        /// <summary>
        /// Returns transitions triggered by a timer which value is equal to 0
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <returns></returns>
        public IEnumerable<TransitionDefinition> GetTransitionsForImmediateExecution(ProcessInstance processInstance, ActivityDefinition activity = null)
        {
            return
                processInstance.ProcessScheme.GetTimerTransitionForActivity(activity ?? processInstance.CurrentActivity, ForkTransitionSearchType.Both)
                    .Where(t => t.Trigger.Timer.Value.Equals(ImmediateTimerValue) && !processInstance.IsParameterExisting(GetTimerValueParameterName(t.Trigger.Timer)));
        }

        private string GetTimerValueParameterName(TimerDefinition timerDefinition)
        {
            return GetTimerValueParameterName(timerDefinition.Name);
        }

        private string GetTimerValueParameterName(string timerName)
        {
            return string.Format("TimerValue_{0}", timerName);
        }

        /// <summary>
        /// Sets new value of named timer
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="timerName">Timer name in Scheme</param>
        /// <param name="newValue">New value of the timer</param>
        public void SetTimerValue(ProcessInstance processInstance, string timerName, DateTime newValue)
        {
            var parameterName = GetTimerValueParameterName(timerName);

            processInstance.SetParameter(parameterName, newValue, ParameterPurpose.Persistence);
        }

        /// <summary>
        /// Sets new value of named timer
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        /// <param name="newValue">New value of the timer</param>
        public Task SetTimerValue(Guid processId, string timerName, DateTime newValue)
        {
            return SetOrResetTimerValue(processId, timerName, newValue);
        }

        /// <summary>
        /// Resets value of named timer
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="timerName">Timer name in Scheme</param>
        public void ResetTimerValue(ProcessInstance processInstance, string timerName)
        {
            var parameterName = GetTimerValueParameterName(timerName);

            if (!processInstance.IsParameterExisting(parameterName))
                return;

            processInstance.SetParameter<DateTime?>(parameterName, null, ParameterPurpose.Persistence);

        }

        /// <summary>
        /// Resets value of named timer
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        public Task ResetTimerValue(Guid processId, string timerName)
        {
            return SetOrResetTimerValue(processId, timerName, null);
        }

        private async Task SetOrResetTimerValue(Guid processId, string timerName, DateTime? newValue)
        {

            var parameterName = GetTimerValueParameterName(timerName);

            var processInstance = _runtime.Builder.GetProcessInstance(processId);
            var oldStatus = _runtime.PersistenceProvider.GetInstanceStatus(processId);
            await _runtime.SetProcessNewStatus(processInstance, ProcessStatus.Running, true).ConfigureAwait(false);

            try
            {
                _runtime.PersistenceProvider.FillProcessParameters(processInstance);

                if (newValue.HasValue)
                    processInstance.SetParameter(parameterName, newValue.Value, ParameterPurpose.Persistence);
                else
                    processInstance.SetParameter<DateTime?>(parameterName, null, ParameterPurpose.Persistence);

                var currentTimers = processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity, ForkTransitionSearchType.Both).ToList();
                var timer = currentTimers.Where(t => t.Trigger.Timer != null && t.Trigger.Timer.Name.Equals(timerName)).Select(t => t.Trigger.Timer).FirstOrDefault();

                if (timer != null && !timer.NotOverrideIfExists)
                {
                    if (_startStopSemaphore.Wait(DefaultWaitTimeout))
                    {
                        try
                        {
                            var wasRunning = !_stopped;
                            _stopped = true;
                            _timer.Change(Timeout.Infinite, Timeout.Infinite);
                            _cancellationTokenSource.Cancel();
                            var needToClearTimer = !newValue.HasValue && (timer.Value.Equals(ImmediateTimerValue) || timer.Value.Equals(InfinityTimerValue));
                            if (needToClearTimer)
                            {
                                var timersIgnoreList = currentTimers.Where(t => t.Trigger.Timer != null && !t.Trigger.Timer.Name.Equals(timerName))
                                    .Select(t => t.Trigger.Timer.Name)
                                    .Distinct()
                                    .ToList();
                                _runtime.PersistenceProvider.ClearTimers(processInstance.ProcessId, timersIgnoreList);
                            }
                            RefreshOrRegisterTimer(processInstance, new List<TimerDefinition> {timer});
                            _runtime.PersistenceProvider.SavePersistenceParameters(processInstance);
                            await _runtime.SetProcessNewStatus(processInstance, oldStatus, true).ConfigureAwait(false);
                            if (wasRunning)
                            {
                                _stopped = false;
                                _cancellationTokenSource = new CancellationTokenSource();
                                RefreshInterval();
                            }
                        }
                        finally
                        {
                            _startStopSemaphore.Release();
                        }
                    }
                    else
                    {
                        throw new TimerManagerException("Can't start change timer value. Wait timeout expired.");
                    }

                }
                else
                {
                    _runtime.PersistenceProvider.SavePersistenceParameters(processInstance);
                   await _runtime.SetProcessNewStatus(processInstance, oldStatus, true).ConfigureAwait(false);
                }
            }
            catch
            {
                await _runtime.SetProcessNewStatus(processInstance, oldStatus, true).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Register all timers for all outgouing timer transitions for current actvity of the specified process.
        /// All timers registered before which are present in transitions will be rewrited except timers marked as NotOverrideIfExists <see cref="TimerDefinition"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be registered</param>
        public void RegisterTimers(ProcessInstance processInstance)
        {
            var usedTransitions = GetUsedTransitions(processInstance);
            RegisterTimers(processInstance, usedTransitions);
        }

        private void RegisterTimers(ProcessInstance processInstance, List<string> usedTransitions)
        {
            var timersToRegister =
                processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity, ForkTransitionSearchType.Both)
                    .Where(t => t.Trigger.Timer != null && !usedTransitions.Contains(t.Name))
                    .Where(
                        t =>
                            (!t.Trigger.Timer.Value.Equals(InfinityTimerValue) && !t.Trigger.Timer.Value.Equals(ImmediateTimerValue)) ||
                            processInstance.IsParameterExisting(GetTimerValueParameterName(t.Trigger.Timer)))
                    .Select(t => t.Trigger.Timer)
                    .ToList();

            if (!timersToRegister.Any())
                return;

            RefreshOrRegisterTimer(processInstance, timersToRegister);
        }

        private void RefreshOrRegisterTimer(ProcessInstance processInstance, List<TimerDefinition> timersToRegister)
        {
            if (!_stopped)
                RefreshInterval(processInstance, timersToRegister);
            else
            {
                RegisterTimers(processInstance, timersToRegister);
            }
        }

        /// <summary>
        /// Clear timers <see cref="ClearTimers"/> and then register new timers <see cref="RegisterTimers"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be cleared an registered</param>
        public void ClearAndRegisterTimers(ProcessInstance processInstance)
        {
            var used = GetUsedTransitions(processInstance);
            ClearTimers(processInstance, used);
            RegisterTimers(processInstance, used);
        }

        private void RefreshInterval(ProcessInstance processInstance, List<TimerDefinition> timersToRegister)
        {
            //_lock.EnterUpgradeableReadLock();

            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                RegisterTimers(processInstance, timersToRegister);
            }
            finally
            {
                RefreshInterval();
                //_lock.ExitUpgradeableReadLock();
            }
        }

        private void RefreshInterval()
        {
            //_lock.EnterWriteLock();

            //try
            //{
            var next = _runtime.PersistenceProvider.GetCloseExecutionDateTime();

            if (!next.HasValue)
                return;

            var now = _runtime.RuntimeDateTimeNow;

            if (now < next)
            {


#if NETCOREAPP
                var doubleTimeout = (next.Value - now).TotalMilliseconds;
                doubleTimeout = Math.Max(0d, doubleTimeout);
                var timeout = Math.Min(4294967294L, (long)doubleTimeout);
                _timer.Change(TimeSpan.FromMilliseconds(timeout),  TimeSpan.FromMilliseconds(Timeout.Infinite));
#else
                var timeout = (long)(next.Value - now).TotalMilliseconds;
               _timer.Change(timeout > 4294967294L ? 4294967294L : timeout, Timeout.Infinite);
#endif
            }
            else
            {
                _timer.Change(0, Timeout.Infinite);
            }
            //}
            //finally
            //{
            //    //_lock.ExitWriteLock();
            //}
        }

        private void RegisterTimers(ProcessInstance processInstance, List<TimerDefinition> timersToRegister)
        {
            var timersWithDate = new List<Tuple<TimerDefinition, DateTime?>>();

            timersToRegister.ForEach(t => timersWithDate.Add(new Tuple<TimerDefinition, DateTime?>(t, GetNextExecutionDateTime(t, processInstance))));


            timersWithDate.ForEach(
                t =>
                {
                    if (t.Item2 != null)
                        _runtime.PersistenceProvider.RegisterTimer(processInstance.ProcessId, t.Item1.Name, t.Item2.Value,
                            t.Item1.NotOverrideIfExists);
                });
        }

        private DateTime? GetNextExecutionDateTime(TimerDefinition timerDefinition, ProcessInstance processInstance = null)
        {
            var timerValueParameterName = GetTimerValueParameterName(timerDefinition);
            if (timerDefinition.Value.Equals(ImmediateTimerValue) || timerDefinition.Value.Equals(InfinityTimerValue))
            {
                if (processInstance == null)
                    return null;

                if (!processInstance.IsParameterExisting(timerValueParameterName))
                    return null;

                return processInstance.GetParameter<DateTime?>(timerValueParameterName);
            }

            if (processInstance != null && processInstance.IsParameterExisting(timerValueParameterName))
            {
                var value = processInstance.GetParameter<DateTime?>(timerValueParameterName);
                if (value != null)
                    return value;
            }

            switch (timerDefinition.Type)
            {
                case TimerType.Date:
                    var date1 = DateTime.Parse(timerDefinition.Value, _runtime.SchemeParsingCulture);
                    return date1.Date;
                case TimerType.DateAndTime:
                    var date2 = DateTime.Parse(timerDefinition.Value, _runtime.SchemeParsingCulture);
                    return date2;
                case TimerType.Interval:
                    var interval = GetInterval(timerDefinition.Value);
                    if (interval <= 0)
                        return null;
                    return _runtime.RuntimeDateTimeNow.AddMilliseconds(interval);
                case TimerType.Time:
                    var now = _runtime.RuntimeDateTimeNow;
                    var date3 = DateTime.Parse(timerDefinition.Value, _runtime.SchemeParsingCulture);
                    date3 = now.Date + date3.TimeOfDay;
                    if (date3.TimeOfDay < now.TimeOfDay)
                        date3 = date3.AddDays(1);
                    return date3;
            }

            return _runtime.RuntimeDateTimeNow;
        }

        protected virtual double GetInterval(string value)
        {
            int res;

            if (int.TryParse(value, out res))
                return res;

            var daysRegex = @"\d*\s*((days)|(day)|(d))(\W|\d|$)";
            var hoursRegex = @"\d*\s*((hours)|(hour)|(h))(\W|\d|$)";
            var minutesRegex = @"\d*\s*((minutes)|(minute)|(m))(\W|\d|$)";
            var secondsRegex = @"\d*\s*((seconds)|(second)|(s))(\W|\d|$)";
            var millisecondsRegex = @"\d*\s*((milliseconds)|(millisecond)|(ms))(\W|\d|$)";

            return new TimeSpan(GetTotal(value, daysRegex), GetTotal(value, hoursRegex), GetTotal(value, minutesRegex), GetTotal(value, secondsRegex),
                GetTotal(value, millisecondsRegex)).TotalMilliseconds;
        }

        private static int GetTotal(string s, string regex)
        {
            int total = 0;
            foreach (var match in Regex.Matches(s, regex, RegexOptions.IgnoreCase))
            {
                int parsed;
                var value = Regex.Match(match.ToString(), @"\d*").Value;
                int.TryParse(value, out parsed);
                total += parsed;
            }

            return total;
        }

        /// <summary>
        /// Clear all registerd timers except present in outgouing timer transitions for current actvity of the specified process and marked as NotOverrideIfExists <see cref="TimerDefinition"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be cleared</param>
        public void ClearTimers(ProcessInstance processInstance)
        {
            var usedTransitions = GetUsedTransitions(processInstance);
            ClearTimers(processInstance, usedTransitions);
        }

        private List<string> GetUsedTransitions(ProcessInstance processInstance)
        {
            var piTree = _runtime.GetProcessInstancesTree(processInstance);
            var usedTransitions = piTree.Root.GetAllChildrenNames();
            return usedTransitions;
        }

        private void ClearTimers(ProcessInstance processInstance, List<string> usedTransitions)
        {
            var timersIgnoreList =
                processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity, ForkTransitionSearchType.Both)
                    .Where(
                        t =>
                            t.Trigger.Timer != null && !usedTransitions.Contains(t.Name) &&
                            t.Trigger.Timer.NotOverrideIfExists).Select(t => t.Trigger.Timer.Name).ToList();

            _runtime.PersistenceProvider.ClearTimers(processInstance.ProcessId, timersIgnoreList);
        }

        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
#if NETCOREAPP
            _timer = new Timer(OnTimer,null,Timeout.Infinite,Timeout.Infinite);
#else
            _timer = new Timer(OnTimer);
#endif
        }


        /// <summary>
        /// Starts the timer
        /// </summary>
        ///<param name="timeout">Wait timeout in milliseconds</param>
        public void Start(int? timeout = null)
        {
            if (_startStopSemaphore.Wait(timeout ?? DefaultWaitTimeout))
            {
                try
                {
                    if (!_stopped)
                        return;

                    _stopped = false;
                    _cancellationTokenSource = new CancellationTokenSource();
                    RefreshInterval();
                }
                finally
                {
                    _startStopSemaphore.Release();
                }
            }
            else
            {
                throw new TimerManagerException("Can't start timer. Wait timeout expired.");
            }
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        ///<param name="timeout">Wait timeout in milliseconds</param>
        public void Stop(int? timeout = null)
        {
            if (_startStopSemaphore.Wait(timeout ?? DefaultWaitTimeout))
            {
                try
                {
                    _stopped = true;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _cancellationTokenSource.Cancel();
                }
                finally
                {
                    _startStopSemaphore.Release();
                }
            }
            else
            {
                throw new TimerManagerException("Can't stop timer. Wait timeout expired.");
            }
        }

        /// <summary>
        /// Refresh interval of the timer
        /// </summary>
        public void Refresh()
        {
            if (_stopped)
                return;
            RefreshInterval();
        }

        private void OnTimer(object state)
        {
            ThreadPool.QueueUserWorkItem(async (obj) =>
            {
                await _startStopSemaphore.WaitAsync().ConfigureAwait(false);

                if (_stopped)
                    return;

                List<TimerToExecute> timers;

                try
                {
                    timers = _runtime.PersistenceProvider.GetTimersToExecute();
                }
                finally
                {
                    _startStopSemaphore.Release();
                }

                try
                {
                    var actions = timers.Select(t =>(Action)(()=> ExecuteTimer(t, _cancellationTokenSource.Token).Wait())).ToArray();
                    Parallel.Invoke(new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = _cancellationTokenSource.Token}, actions);
                    //await Task.WhenAll(timers.Select(t => ExecuteTimer(t, _cancellationTokenSource.Token)));
                }
                finally
                {
                    RefreshInterval();
                }

            });
        }

        private async Task ExecuteTimer(TimerToExecute timer, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                _runtime.PersistenceProvider.ClearTimerIgnore(timer.TimerId);
                return;
            }

            void LogException(Exception ex)
            {
                _runtime.LogError("Execute timer error", new Dictionary<string, string>()
                {
                    {"Message", ex.Message},
                    {"StackTrace", ex.StackTrace},
                    {"ProcessId", timer.ProcessId.ToString("D")},
                    {"TimerId", timer.TimerId.ToString("D")},
                    {"TimerName", timer.Name},
                });
            }

            void LogExceptionOrThrow(Exception ex)
            {
                if (_runtime.Logger != null)
                {
                    LogException(ex);
                }
                else
                {
                    throw ex;
                }
            }

            void LogExceptionOrSuppress(Exception ex)
            {
                if (_runtime.Logger != null)
                {
                    LogException(ex);
                }
            }

            try
            {
                var timerToExecute = timer;
                await _runtime.ExecuteTimerAsync(timerToExecute.ProcessId, timerToExecute.Name).ConfigureAwait(false);
            }
            // After implementing logger insert log here
            catch (ImpossibleToSetStatusException ex)
            {
                LogExceptionOrSuppress(ex);
            }
            catch (ProcessNotFoundException ex)
            {
                LogExceptionOrSuppress(ex);
            }
            catch (StatusNotDefinedException ex)
            {
                LogExceptionOrSuppress(ex);
            }
            catch (Exception ex)
            {
                LogExceptionOrThrow(ex);
            }
            finally
            {
                try
                {
                    _runtime.PersistenceProvider.ClearTimer(timer.TimerId);
                }
                catch (Exception ex)
                {
                    LogExceptionOrThrow(ex);
                }
            }
        }

        public List<TimerToRegister> GetTimersToRegister(ProcessDefinition processDefinition, string activityName)
        {
            return GetTimerToRegisters(processDefinition, activityName);
        }

        public List<TimerToRegister> GetTimersToRegister(ProcessInstance processInstance, string activityName)
        {
            return GetTimerToRegisters(processInstance.ProcessScheme, activityName, processInstance);
        }

        private List<TimerToRegister> GetTimerToRegisters(ProcessDefinition processDefinition, string activityName, ProcessInstance processInstance = null)
        {
            var timerTransitions = processDefinition.GetTimerTransitionForActivity(processDefinition.FindActivity(activityName), ForkTransitionSearchType.Both);
            var timerDefinitions = timerTransitions.Select(t => t.Trigger.Timer).GroupBy(t => t.Name).Select(g => g.First()).ToList();
            return
                timerDefinitions.Select(td => new { Date = GetNextExecutionDateTime(td, processInstance), td.Name })
                    .Where(t => t.Date.HasValue)
                    .Select(t => new TimerToRegister { ExecutionDateTime = t.Date.Value, Name = t.Name, ProcessId = processInstance != null ? processInstance.ProcessId : Guid.Empty})
                    .ToList();
        }
    }
}
