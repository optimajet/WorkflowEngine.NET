using Microsoft.Data.Sqlite;

namespace OptimaJet.Workflow.SQLite.Models;

/// <summary>
/// Represents a parameterized SQL query consisting of the command text and its associated parameters.
/// </summary>
public class QueryDefinition
{
    /// <summary>
    /// Gets or sets the SQL command text.
    /// </summary>
    public string Query { get; set; }

    /// <summary>
    /// Gets or sets the list of <see cref="SqliteParameter"/> objects for the query.
    /// </summary>
    public List<SqliteParameter> Parameters { get; set; }
}
