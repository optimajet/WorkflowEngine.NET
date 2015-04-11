using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Interface of a timer manager, which control timers functioning inside a workflow runtime
    /// </summary>
    public interface ITimerManager
    {
        /// <summary>
        /// Register all timers for all outgouing timer transitions for current actvity of the specified process.
        /// All timers registered before which are present in transitions will be rewrited except timers marked as NotOverrideIfExists <see cref="TimerDefinition"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be registered</param>
        void RegisterTimers(ProcessInstance processInstance);

        /// <summary>
        /// Clear timers <see cref="ClearTimers"/> and then register new timers <see cref="RegisterTimers"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be cleared an registered</param>
        void ClearAndRegisterTimers(ProcessInstance processInstance);

        /// <summary>
        /// Clear all registerd timers except present in outgouing timer transitions for current actvity of the specified process and marked as NotOverrideIfExists <see cref="TimerDefinition"/>
        /// </summary>
        /// <param name="processInstance">Process instance whose timers need to be cleared</param>
        void ClearTimers(ProcessInstance processInstance);

        void Init(WorkflowRuntime runtime);

        /// <summary>
        /// Start the timer
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the timer
        /// </summary>
        void Stop();

        /// <summary>
        /// Refresh interval of the timer
        /// </summary>
        void Refresh();
    }
}
