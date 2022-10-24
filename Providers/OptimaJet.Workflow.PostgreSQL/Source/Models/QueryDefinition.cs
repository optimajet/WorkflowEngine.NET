using System.Collections.Generic;
using Npgsql;

namespace OptimaJet.Workflow.PostgreSQL.Models;

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
    public List<NpgsqlParameter> Parameters { get; set; }
}
