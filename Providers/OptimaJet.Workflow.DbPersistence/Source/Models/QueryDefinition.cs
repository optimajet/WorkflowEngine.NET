using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace OptimaJet.Workflow.MSSQL.Models;

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
    public List<SqlParameter> Parameters { get; set; }
}
