using System;
using System.Collections.Generic;
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
        SchemeDefinition<TSchemeMedium> GetProcessSchemeByProcessId(Guid processId);
        /// <summary>
        /// Gets not parsed scheme by id
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
        SchemeDefinition<TSchemeMedium> GetProcessSchemeBySchemeId(Guid schemeId);

        /// <summary>
        /// Gets not parsed scheme by scheme name and parameters    
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">Parameters for creating the scheme</param>
        /// <param name="rootSchemeId">Id of the root scheme in case of subprocess</param>
        /// <param name="ignoreObsolete">True if you need to ignore obsolete schemes</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
        SchemeDefinition<TSchemeMedium> GetProcessSchemeWithParameters(string schemeCode,
            string parameters,
            Guid? rootSchemeId,
            bool ignoreObsolete);

       
        /// <summary>
        /// Gets not parsed scheme by scheme name  
        /// </summary>
        /// <param name="code">Name of the scheme</param>
        /// <returns>Not parsed scheme of the process</returns>
        /// <exception cref="SchemeNotFoundException"></exception>
        TSchemeMedium GetScheme(string code);

        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="scheme">Not parsed scheme of the process</param>
        /// <exception cref="SchemeAlredyExistsException"></exception>
        void SaveScheme(SchemeDefinition<TSchemeMedium> scheme);

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">Parameters for creating the scheme</param>
        void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters);

        /// <summary>
        /// Sets sign IsObsolete to the scheme
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        void SetSchemeIsObsolete(string schemeCode);


        /// <summary>
        /// Saves scheme to a store
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="scheme">Not parsed scheme</param>
        void SaveScheme(string schemeCode, string scheme);
    }
}
