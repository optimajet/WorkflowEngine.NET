using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Persistence
{

    /// <summary>
    /// Interface of a persistence provider, which provide storing of schemes
    /// </summary>
    /// <typeparam name="TSchemeMedium">Type of not parsed scheme</typeparam>
    public interface ISchemePersistenceProvider<TSchemeMedium> where TSchemeMedium : class
    {
        /// <summary>
        /// Gets not parsed scheme of the process by process id
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="ProcessNotFoundException"></exception>
        /// <exception cref="SchemeNotFoundException"></exception>
        Task<SchemeDefinition<TSchemeMedium>> GetProcessSchemeByProcessIdAsync(Guid processId);

        /// <summary>
        /// Gets not parsed scheme by id
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
         Task<SchemeDefinition<TSchemeMedium>> GetProcessSchemeBySchemeIdAsync(Guid schemeId);

        /// <summary>
        /// Gets not parsed scheme by scheme name and parameters    
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">Parameters for creating the scheme</param>
        /// <param name="rootSchemeId">Id of the root scheme in case of subprocess</param>
        /// <param name="ignoreObsolete">True if you need to ignore obsolete schemes</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
         Task<SchemeDefinition<TSchemeMedium>> GetProcessSchemeWithParametersAsync(string schemeCode,
            string parameters,
            Guid? rootSchemeId,
            bool ignoreObsolete);


        /// <summary>
        /// Gets not parsed scheme by scheme name  
        /// </summary>
        /// <param name="code">Name of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
         Task<TSchemeMedium> GetSchemeAsync(string code);

        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="scheme">Not parsed scheme of the process</param>
        /// <exception cref="SchemeAlreadyExistsException"></exception>
         Task<SchemeDefinition<TSchemeMedium>> SaveSchemeAsync(SchemeDefinition<TSchemeMedium> scheme);

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">Parameters for creating the scheme</param>
        Task SetSchemeIsObsoleteAsync(string schemeCode, IDictionary<string, object> parameters);

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        Task SetSchemeIsObsoleteAsync(string schemeCode);


        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="inlinedSchemes">Scheme codes to be inlined into this scheme</param>
        /// <param name="scheme">Not parsed scheme</param>
        /// <param name="canBeInlined">if true - this scheme can be inlined into another schemes</param>
        Task SaveSchemeAsync(string schemeCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags);

        /// <summary>
        /// Returns the list of scheme codes that can be inlined into other schemes
        /// </summary>
        /// <returns>The list of scheme codes</returns>
         Task<List<string>> GetInlinedSchemeCodesAsync();

        /// <summary>
        /// Returns the list of scheme codes into which the scheme with the given code has been inlined
        /// </summary>
        /// <param name="schemeCode">Inlined scheme code</param>
        /// <returns></returns>
        Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode);

        #region tags
        /// <summary>
        /// Returns the list of scheme codes into which the scheme with the given tags
        /// </summary>
        Task<List<string>> SearchSchemesByTagsAsync(params string[] tags);
        /// <summary>
        /// Returns the list of scheme codes into which the scheme with the given tags
        /// </summary>
        Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags);
        /// <summary>
        /// Add tags to scheme with the given schemeCode
        /// </summary>
        Task AddSchemeTagsAsync(string schemeCode, params string[] tags);
        /// <summary>
        /// Add tags to scheme with the given schemeCode
        /// </summary>
        Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags);
        /// <summary>
        /// Remove tags from scheme with the given schemeCode
        /// </summary>
        Task RemoveSchemeTagsAsync(string schemeCode, params string[] tags);
        /// <summary>
        /// Remove tags from scheme with the given schemeCode
        /// </summary>
        Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags);
        /// <summary>
        /// Set tags to scheme with the given schemeCode
        /// </summary>
        Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags);
        /// <summary>
        /// Set tags to scheme with the given schemeCode
        /// </summary>
        Task SetSchemeTagsAsync(string schemeCode, params string[] tags);
        #endregion tags
    }
}
