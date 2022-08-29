using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessScheme : DbObject<ProcessSchemeEntity>
    {
        public WorkflowProcessScheme(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessScheme", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.DefiningParameters)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.DefiningParametersHash)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.IsObsolete), Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.SchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Scheme)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.AllowedActivities)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.StartingTransition)}
            });
        }

        public async Task<ProcessSchemeEntity[]> SelectAsync(NpgsqlConnection connection, string schemeCode, string definingParametersHash, bool? isObsolete, Guid? rootSchemeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE \"{nameof(ProcessSchemeEntity.SchemeCode)}\" = @schemecode " + 
                                $"AND \"{nameof(ProcessSchemeEntity.DefiningParametersHash)}\" = @dphash";

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += $" AND \"{nameof(ProcessSchemeEntity.IsObsolete)}\" = TRUE";
                }
                else
                {
                    selectText += $" AND \"{nameof(ProcessSchemeEntity.IsObsolete)}\" = FALSE";
                }
            }

            var pSchemecode = new NpgsqlParameter("schemecode", NpgsqlDbType.Varchar) {Value = schemeCode};

            var pDphash = new NpgsqlParameter("dphash", NpgsqlDbType.Varchar) {Value = definingParametersHash};

            if (rootSchemeId.HasValue)
            {
                selectText += $" AND \"{nameof(ProcessSchemeEntity.RootSchemeId)}\" = @rootschemeid";
                var pRootSchemeId = new NpgsqlParameter("rootschemeid", NpgsqlDbType.Uuid) {Value = rootSchemeId.Value};

                return await SelectAsync(connection, selectText, pSchemecode, pDphash, pRootSchemeId).ConfigureAwait(false);
            }

            selectText += $" AND \"{nameof(ProcessSchemeEntity.RootSchemeId)}\" IS NULL";
            return await SelectAsync(connection, selectText, pSchemecode, pDphash).ConfigureAwait(false);
        }

        public async Task<int> SetObsoleteAsync(NpgsqlConnection connection, string schemeCode)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"\"{nameof(ProcessSchemeEntity.IsObsolete)}\" = TRUE WHERE " + 
                             $"\"{nameof(ProcessSchemeEntity.SchemeCode)}\" = @schemecode " + 
                             $"OR \"{nameof(ProcessSchemeEntity.RootSchemeCode)}\" = @schemecode";
            var p = new NpgsqlParameter("schemecode", NpgsqlDbType.Varchar) {Value = schemeCode};

            return await ExecuteCommandNonQueryAsync(connection, command, p).ConfigureAwait(false);
        }

        public async Task<int> SetObsoleteAsync(NpgsqlConnection connection, string schemeCode, string definingParametersHash)
        {
            string command =
                $"UPDATE {ObjectName} SET \"{nameof(ProcessSchemeEntity.IsObsolete)}\" = TRUE " + 
                $"WHERE (\"{nameof(ProcessSchemeEntity.SchemeCode)}\" = @schemecode " + 
                $"OR \"{nameof(ProcessSchemeEntity.RootSchemeCode)}\" = @schemecode) " + 
                $"AND \"{nameof(ProcessSchemeEntity.DefiningParametersHash)}\" = @dphash";

            var p = new NpgsqlParameter("schemecode", NpgsqlDbType.Varchar) {Value = schemeCode};
            var p2 = new NpgsqlParameter("dphash", NpgsqlDbType.Varchar) {Value = definingParametersHash};

            return await ExecuteCommandNonQueryAsync(connection, command, p, p2).ConfigureAwait(false);
        }
        
        public static async Task DeleteUnusedAsync(NpgsqlConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
            
            using var transaction = connection.BeginTransaction();
            using var command = new NpgsqlCommand("SELECT \"DropUnusedWorkflowProcessScheme\"()", connection)
            {
                Transaction = transaction
            };
            
            var status = (int) await command.ExecuteScalarAsync().ConfigureAwait(false);
            
            if (status != 0)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw new Exception("Failed to clean up unused WorkflowProcessSchemes ");
            }
            await transaction.CommitAsync().ConfigureAwait(false);
        }
    }
}
