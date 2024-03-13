using Microsoft.Data.Sqlite;

namespace OptimaJet.Workflow.SQLite.Models;

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
    public List<SqliteParameter> Parameters { get; set; }
}
