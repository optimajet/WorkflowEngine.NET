using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
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
    public sealed class TimerManager : ITimerManager
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private WorkflowRuntime _runtime;

        private Timer _timer;

        private bool _stopped = true;

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
                processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity,ForkTransitionSearchType.Both)
                    .Where(t => t.Trigger.Timer != null && !usedTransitions.Contains(t.Name))
                    .Select(t => t.Trigger.Timer)
                    .ToList();

            if (!timersToRegister.Any())
                return;

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
            _lock.EnterUpgradeableReadLock();

            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                RegisterTimers(processInstance, timersToRegister);

                RefreshInterval();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        private void RefreshInterval()
        {
            _lock.EnterWriteLock();

            try
            {
                var next = _runtime.PersistenceProvider.GetCloseExecutionDateTime();

                if (!next.HasValue)
                    return;

                var now = _runtime.RuntimeDateTimeNow;

                if (now < next)
                {
                    var timeout = (long) (next.Value - now).TotalMilliseconds;
                    _timer.Change(timeout > 4294967294L ? 4294967294L : timeout, Timeout.Infinite);
                }
                else
                {
                    _timer.Change(0, Timeout.Infinite);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void RegisterTimers(ProcessInstance processInstance, List<TimerDefinition> timersToRegister)
        {
            timersToRegister.ForEach(
                t =>
                    _runtime.PersistenceProvider.RegisterTimer(processInstance.ProcessId, t.Name, GetNextExecutionDateTime(t),
                        t.NotOverrideIfExists));
        }

        private DateTime GetNextExecutionDateTime(TimerDefinition timerDefinition)
        {
            switch (timerDefinition.Type)
            {
                case TimerType.Date:
                    var date1 = DateTime.Parse(timerDefinition.Value, _runtime.SchemeParsingCulture);
                    return date1.Date;
                case TimerType.DateAndTime:
                    var date2 = DateTime.Parse(timerDefinition.Value, _runtime.SchemeParsingCulture);
                    return date2;
                case TimerType.Interval:
                    var interval = Int32.Parse(timerDefinition.Value);
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
            _timer = new Timer(OnTimer);
        }


        /// <summary>
        /// Start the timer
        /// </summary>
        public void Start()
        {
            if (!_stopped)
                return;

            _stopped = false;
            RefreshInterval();
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop()
        {
            _stopped = true;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
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
            var timers = _runtime.PersistenceProvider.GetTimersToExecute();
            foreach (var timer in timers)
            {
                try
                {
                    _runtime.ExecuteTimer(timer.ProcessId,timer.Name);
                }
                finally
                {
                    _runtime.PersistenceProvider.ClearTimer(timer.TimerId);
                }
            }
            RefreshInterval();
        }
    }
}
