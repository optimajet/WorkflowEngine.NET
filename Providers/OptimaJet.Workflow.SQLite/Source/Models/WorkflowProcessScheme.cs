using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    public class WorkflowProcessScheme : DbObject<ProcessSchemeEntity>
    {
        public WorkflowProcessScheme(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessScheme", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.IsObsolete), Type = DbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.SchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Scheme)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.AllowedActivities)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.StartingTransition)}
            });
        }

        public async Task<ProcessSchemeEntity[]> SelectAsync(SqliteConnection connection, string schemeCode, bool? isObsolete, Guid? rootSchemeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(ProcessSchemeEntity.SchemeCode)} = @schemecode ";

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += $" AND {nameof(ProcessSchemeEntity.IsObsolete)} = TRUE";
                }
                else
                {
                    selectText += $" AND {nameof(ProcessSchemeEntity.IsObsolete)} = FALSE";
                }
            }

            var pSchemecode = new SqliteParameter("schemecode", DbType.String) {Value = schemeCode};

            if (rootSchemeId.HasValue)
            {
                selectText += $" AND {nameof(ProcessSchemeEntity.RootSchemeId)} = @rootschemeid";
                var pRootSchemeId = new SqliteParameter("rootschemeid", DbType.String) {Value = ToDbValue(rootSchemeId.Value, DbType.Guid)};

                return await SelectAsync(connection, selectText, pSchemecode, pRootSchemeId).ConfigureAwait(false);
            }

            selectText += $" AND {nameof(ProcessSchemeEntity.RootSchemeId)} IS NULL";
            return await SelectAsync(connection, selectText, pSchemecode).ConfigureAwait(false);
        }

        public async Task<int> SetObsoleteAsync(SqliteConnection connection, string schemeCode)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"{nameof(ProcessSchemeEntity.IsObsolete)} = TRUE WHERE " + 
                             $"{nameof(ProcessSchemeEntity.SchemeCode)} = @schemecode " + 
                             $"OR {nameof(ProcessSchemeEntity.RootSchemeCode)} = @schemecode";
            var p = new SqliteParameter("schemecode", DbType.String) {Value = schemeCode};

            return await ExecuteCommandNonQueryAsync(connection, command, p).ConfigureAwait(false);
        }

        public async Task DeleteUnusedAsync(SqliteConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
            
            using var transaction = connection.BeginTransaction();
            const string deleteText = "DELETE FROM WorkflowProcessScheme AS wps " +
                                      "WHERE wps.IsObsolete = 1 AND NOT EXISTS " +
                                      "(SELECT * FROM WorkflowProcessInstance AS wpi WHERE wpi.SchemeId = wps.Id)";
            
            await ExecuteCommandNonQueryAsync(connection, deleteText, transaction).ConfigureAwait(false);

            const string selectText = "SELECT COUNT(*) " +
                                      "FROM WorkflowProcessInstance AS wpi " + 
                                      "LEFT OUTER JOIN WorkflowProcessScheme AS wps ON wpi.SchemeId = wps.Id " +
                                      "WHERE wps.Id IS NULL";

            var result = await ExecuteCommandScalarAsync(connection, selectText, transaction).ConfigureAwait(false);
            result = (result == DBNull.Value) ? null : result;
            int rowcount = Convert.ToInt32(result);
            
            if (rowcount != 0)
            {
                transaction.Rollback();
                throw new Exception("Failed to clean up unused WorkflowProcessSchemes");
            }
            
            transaction.Commit();
        }
    }
}
