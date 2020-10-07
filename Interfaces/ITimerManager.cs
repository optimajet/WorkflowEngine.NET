using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Utils;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Interface of a timer manager, which control timers functioning inside a workflow runtime
    /// </summary>
    public interface ITimerManager
    {
        /// <summary>
        /// Value of Unspecified Timer which indicates that the timer transition will be executed immediately
        /// </summary>
        string ImmediateTimerValue { get; }

        /// <summary>
        /// Value of Unspecified Timer which indicates that the timer transition will be never executed
        /// </summary>
        string InfinityTimerValue { get; }
    
        /// <summary>
        /// Raises when the timer value must be obtained 
        /// </summary>
        event EventHandler<NeedTimerValueEventArgs> OnNeedTimerValue;
        
        /// <summary>
        /// Raises when the timer value must be obtained 
        /// </summary>
        event AsyncEventHandler<NeedTimerValueEventArgs> OnNeedTimerValueAsync;

        /// <summary>
        /// Sends request for timer value for all timer transitions that are outgoing from the CurrentActivity if timer value is equal 0 or -1
        /// </summary>
        /// <param name="activity">Activity to get outbound transition, if null the CurrentActivity will be used</param>
        /// <param name="processInstance">Process instance</param>
        Task RequestTimerValueAsync(ProcessInstance processInstance, ActivityDefinition activity = null);

        /// <summary>
        /// Returns transitions triggered by a timer which value is equal to 0
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="activity">Activity to get outbound transition, if null the CurrentActivity will be used</param>
        /// <returns></returns>
        IEnumerable<TransitionDefinition> GetTransitionsForImmediateExecution(ProcessInstance processInstance, ActivityDefinition activity = null);

        /// <summary>
        /// Sets new value of named timer
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="timerName">Timer name in Scheme</param>
        /// <param name="newValue">New value of the timer</param>
        void SetTimerValue(ProcessInstance processInstance, string timerName, DateTime newValue);

        /// <summary>
        /// Sets new value of named timer
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        /// <param name="newValue">New value of the timer</param>
        Task SetTimerValue(Guid processId, string timerName, DateTime newValue);

        /// <summary>
        /// Resets value of named timer
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="timerName">Timer name in Scheme</param>
        void ResetTimerValue(ProcessInstance processInstance, string timerName);

        /// <summary>
        /// Resets value of named timer
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        Task ResetTimerValue(Guid processId, string timerName);

        /// <summary>
        /// Register all timers for all outgouing timer transitions for current actvity of the specified process.
        /// All timers registered before which are present in transitions will be rewrited except timers marked as NotOverrideIfExists <see cref="TimerDefinition"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be registered</param>
        Task RegisterTimersAsync(ProcessInstance processInstance);


        List<TimerToRegister> GetTimersToRegister(ProcessDefinition processDefinition, string activityName);
        List<TimerToRegister> GetTimersToRegister(ProcessInstance processInstance, string activityName);
         
        /// <summary>
        /// Clear timers <see cref="ClearTimers"/> and then register new timers <see cref="RegisterTimers"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be cleared an registered</param>
        Task ClearAndRegisterTimersAsync(ProcessInstance processInstance);

        /// <summary>
        /// Clear all registerd timers except present in outgouing timer transitions for current actvity of the specified process and marked as NotOverrideIfExists <see cref="TimerDefinition"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be cleared</param>
        Task ClearTimersAsync(ProcessInstance processInstance);

        void Init(WorkflowRuntime runtime);

        /// <summary>
        /// Starts the timer
        /// </summary>
        ///<param name="timeout">Wait timeout in milliseconds</param>
        Task StartAsync(int? timeout = null);

        /// <summary>
        /// Stops the timer
        /// </summary>
        ///<param name="timeout">Wait timeout in milliseconds</param>
        void Stop(int? timeout = null);

        /// <summary>
        /// Refresh interval of the timer
        /// </summary>
        void Refresh();

        bool IsSupportsMultiServer { get; }
    }
}
