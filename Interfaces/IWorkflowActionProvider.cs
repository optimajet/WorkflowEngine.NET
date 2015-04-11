using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Interface of a action provider, which provide execution of actions specified in activities <see cref="ActivityDefinition.Implementation"/> and <see cref="ActivityDefinition.PreExecutionImplementation"/>
    /// and execution of conditions <see cref="ConditionDefinition.Action"/>
    /// </summary>
    public interface IWorkflowActionProvider
    {
        /// <summary>
        /// Execute action
        /// </summary>
        /// <param name="name">Name of the action to execute</param>
        /// <param name="processInstance">Reference to ProcessInstance from which action is executed <see cref="ProcessInstance"/></param>
        /// <param name="runtime">Reference to WorkflowRuntime object which managed specified process instance <see cref="WorkflowRuntime"/></param>
        /// <param name="actionParameter">Additional action parameter <see cref="ActionDefinitionReference.ActionParameter"/></param>
        void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter);

        /// <summary>
        /// Check condition
        /// </summary>
        /// <param name="name">Name of the condition to check</param>
        /// <param name="processInstance">Reference to ProcessInstance from which condition is checked <see cref="ProcessInstance"/></param>
        /// <param name="runtime">Reference to WorkflowRuntime object which managed specified process instance <see cref="WorkflowRuntime"/></param>
        /// <param name="actionParameter">Additional action parameter <see cref="ActionDefinitionReference.ActionParameter"/></param>
        /// <returns>Condition result</returns>
        bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter);

        /// <summary>
        /// Return all user conditions and actions names
        /// </summary>
        /// <returns>List of conditions and actions names</returns>
        List<string> GetActions();
    }
}
