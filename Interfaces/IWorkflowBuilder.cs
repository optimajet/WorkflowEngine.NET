using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Builder
{
    /// <summary>
    /// Interface of a workflow builder, which convert not parsed process scheme <see cref="SchemeDefinition{T}"/> to the object model of a scheme of a process <see cref="ProcessDefinition"/>
    /// </summary>
    public interface IWorkflowBuilder
    {
        /// <summary>
        /// Create new instance of the process.
        /// </summary>
        /// <param name="workflowRuntime">Instance of workflow runtime which creates an process instance</param>
        /// <param name="processId">Process id</param>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        /// /// <param name="tenantId">Tenant id</param>
        /// <returns>ProcessInstance object</returns>
        Task<ProcessInstance> CreateNewProcessAsync(WorkflowRuntime workflowRuntime, Guid processId,
            string schemeCode,
            IDictionary<string, object> parameters,
            string tenantId);


        Task<ProcessInstance> CreateNewSubprocessAsync(WorkflowRuntime workflowRuntime, Guid processId,
            ProcessInstance parentProcessInstance,
            TransitionDefinition startingTransition
        );


        /// <summary>
        /// Create new scheme for existing process
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns>ProcessDefinition object</returns>
        Task<ProcessDefinition> CreateNewProcessSchemeAsync(string schemeCode, IDictionary<string, object> parameters);

        Task<ProcessDefinition> CreateNewSubprocessSchemeAsync(ProcessDefinition parentProcessScheme,
            TransitionDefinition startingTransition);

        /// <summary>
        /// Returns existing process instance
        /// </summary>
        /// <param name="workflowRuntime">Workflow runtime instance</param>
        /// <param name="processId">Process id</param>
        /// <returns>ProcessInstance object</returns>
        Task<ProcessInstance> GetProcessInstanceAsync(WorkflowRuntime workflowRuntime, Guid processId);

        /// <summary>
        /// Returns process scheme by specific id, if scheme not exists creates it 
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        Task<ProcessDefinition> GetProcessSchemeAsync(Guid schemeId);

        /// <summary>
        /// Sets the cache to store parsed ProcessDefinition objects <see cref="ProcessDefinition"/> 
        /// </summary>
        /// <param name="cache">Instance of cache object</param>
        void SetCache(IParsedProcessCache cache);

        /// <summary>
        /// Removes the cache to store parsed ProcessDefinition objects <see cref="ProcessDefinition"/> 
        /// </summary>
        void RemoveCache();

        /// <summary>
        /// Returns process scheme by specific name, if scheme not exists creates it
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        Task<ProcessDefinition> GetProcessSchemeAsync(string schemeCode);

        /// <summary>
        /// Returns process scheme by specific name and parameters for creating the scheme of the process, if scheme not exists creates it
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        /// <returns>ProcessDefinition object</returns>
        Task<ProcessDefinition> GetProcessSchemeAsync(string schemeCode, IDictionary<string, object> parameters);

        /// <summary>
        /// Set IsObsolete sign to the scheme with specific name and parameters for creating the scheme of the process
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        Task SetSchemeIsObsoleteAsync(string schemeCode, Dictionary<string, object> parameters);

        /// <summary>
        /// Set IsObsolete sign to the scheme with specific name
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        Task SetSchemeIsObsoleteAsync(string schemeCode);

        /// <summary>
        /// Returns existing process scheme directly from scheme persistence store
        /// </summary>
        /// <param name="code">Name of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        Task<ProcessDefinition> GetProcessSchemeForDesignerAsync(string code);

        /// <summary>
        /// Saves process scheme to scheme persistence store
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="pd">Object representation of the scheme</param>
        /// <returns>
        /// success - true if scheme validation was success,
        /// errors - validation errors,
        /// failedStep - the name of failed build step
        /// </returns>
        Task<(bool success, List<string> errors,string failedStep)> SaveProcessSchemeAsync(string schemeCode, ProcessDefinition pd);

        /// <summary>
        /// Parses process scheme from the string
        /// </summary>
        /// <param name="scheme">String representation of not parsed scheme</param>
        /// <param name="schemeParsingType">Type of parsing strict or soft. Uses only for upload operations where we need softer scheme check</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition Parse(string scheme, SchemeParsingType schemeParsingType = SchemeParsingType.Strict);

        /// <summary>
        /// Serialize process scheme to the string
        /// </summary>
        /// <param name="processDefinition">SProcessDefinition object</param>
        /// <returns>String representation of not parsed scheme</returns>
        string Serialize(ProcessDefinition processDefinition);

        /// <summary>
        /// Adds a build step into workflow builder
        /// </summary>
        /// <param name="order">Order in position</param>
        /// <param name="buildStepPosition">Indicates whether the build step is added after the system steps or before the system steps.</param>
        /// <param name="step">Build step</param>
        void AddBuildStep(int order, BuildStepPosition buildStepPosition, BuildStep step);

        /// <summary>
        /// Generates new Process Definition, doesn't save it in a database, doesn't use cache, doesn't execute build steps
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        /// <returns>Generated process definition</returns>
        Task<ProcessDefinition> GenerateProcessDefinitionAsync(string schemeCode, IDictionary<string, object> parameters);

        /// <summary>
        /// Returns true if the builder contains a build step with the name
        /// </summary>
        /// <param name="name">Name of build step</param>
        /// <returns></returns>
        bool ContainsBuildStep(string name);
        
        /// <summary>
        /// Returns the list of scheme codes that can be inlined into other schemes
        /// </summary>
        /// <returns>The list of scheme codes</returns>
        Task<List<string>> GetInlinedSchemeCodesAsync();
        
        /// <summary>
        /// Returns the list of scheme codes into which the scheme with the given code has been inlined
        /// </summary>
        /// <param name="schemeCode">Inlined scheme code</param>
        /// <returns>The list of scheme codes into which the scheme with the given code has been inlined</returns>
        Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode);
       
        /// <summary>
        /// Remove tags from scheme with the given schemeCode
        /// </summary>
        Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags);
        
        /// <summary>
        /// Set tags to scheme with the given schemeCode
        /// </summary>
        Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags);
        
        /// <summary>
        /// Returns the list of scheme codes into which the scheme with the given tags
        /// </summary>
        Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags);
     
        /// <summary>
        /// Add tags to scheme with the given schemeCode
        /// </summary>
        Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags);

        /// <summary>
        /// Get the list of inlined schemes for a given scheme
        /// </summary>
        /// <param name="pd">Object representation of the scheme</param>
        /// <returns>
        /// codes - list of inlined scheme codes,
        /// errors - validation errors,
        /// failedStep - the name of failed build step
        /// </returns>
        Task<(List<string> codes,List<string> errors,string failedStep)> GetInlineSchemesAsync(ProcessDefinition pd);
    }
}
