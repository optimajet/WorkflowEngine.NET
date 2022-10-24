using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle.Models;

/// <summary>
/// Definition of query
/// </summary>
public class QueryDefinition
{
    /// <summary>
    /// Query text
    /// </summary>
    public string Query { get; set; }
    
    /// <summary>
    /// Query parameters
    /// </summary>
    public List<OracleParameter> Parameters { get; set; }
}
