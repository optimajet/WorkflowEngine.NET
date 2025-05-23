using System.Collections.Generic;
using MySqlConnector;

namespace OptimaJet.Workflow.MySQL.Models;

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
    public List<MySqlParameter> Parameters { get; set; }
}
