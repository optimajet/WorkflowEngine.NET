using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessScheme : DbObject<ProcessSchemeEntity>
    {
        public WorkflowProcessScheme(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessScheme", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.DefiningParameters)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.DefiningParametersHash)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.IsObsolete), Type = SqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.SchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Scheme), Type = SqlDbType.NVarChar, Size = -1},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.AllowedActivities)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.StartingTransition)}
            });
        }

        public async Task<ProcessSchemeEntity[]> SelectAsync(SqlConnection connection, string schemeCode, string definingParametersHash,
            bool? isObsolete, Guid? rootSchemeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE [{nameof(ProcessSchemeEntity.SchemeCode)}] = @schemecode " +
                                $"AND [{nameof(ProcessSchemeEntity.DefiningParametersHash)}] = @dphash";

            var pSchemeCode = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            var pHash = new SqlParameter("dphash", SqlDbType.NVarChar) {Value = definingParametersHash};

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += $" AND [{nameof(ProcessSchemeEntity.IsObsolete)}] = 1";
                }
                else
                {
                    selectText += $" AND [{nameof(ProcessSchemeEntity.IsObsolete)}] = 0";
                }
            }

            if (rootSchemeId.HasValue)
            {
                selectText += $" AND [{nameof(ProcessSchemeEntity.RootSchemeId)}] = @drootschemeid";
                var pRootSchemeId = new SqlParameter("drootschemeid", SqlDbType.UniqueIdentifier) {Value = rootSchemeId.Value};

                return await SelectAsync(connection, selectText, pSchemeCode, pHash, pRootSchemeId).ConfigureAwait(false);
            }

            selectText += $" AND [{nameof(ProcessSchemeEntity.RootSchemeId)}] IS NULL";
            return await SelectAsync(connection, selectText, pSchemeCode, pHash).ConfigureAwait(false);
        }

        public async Task<int> SetObsoleteAsync(SqlConnection connection, string schemeCode)
        {
            string command = $"UPDATE {ObjectName} " +
                             $"SET [{nameof(ProcessSchemeEntity.IsObsolete)}] = 1 " +
                             $"WHERE [{nameof(ProcessSchemeEntity.SchemeCode)}] = @schemecode " +
                             $"OR [{nameof(ProcessSchemeEntity.RootSchemeCode)}] = @schemecode";
            var p = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            return await ExecuteCommandNonQueryAsync(connection, command, p).ConfigureAwait(false);
        }

        public async Task<int> SetObsoleteAsync(SqlConnection connection, string schemeCode, string definingParametersHash)
        {
            string command = $"UPDATE {ObjectName} SET [{nameof(ProcessSchemeEntity.IsObsolete)}] = 1 " +
                             $"WHERE ([{nameof(ProcessSchemeEntity.SchemeCode)}] = @schemecode " +
                             $"OR [{nameof(ProcessSchemeEntity.RootSchemeCode)}] = @schemecode) " +
                             $"AND [{nameof(ProcessSchemeEntity.DefiningParametersHash)}] = @dphash";

            var p = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            var p2 = new SqlParameter("dphash", SqlDbType.NVarChar) {Value = definingParametersHash};

            return await ExecuteCommandNonQueryAsync(connection, command, p, p2).ConfigureAwait(false);
        }

        public static async Task DeleteUnusedAsync(SqlConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
            
            using var transaction = connection.BeginTransaction();
            using var cmd = new SqlCommand("dbo.DropUnusedWorkflowProcessScheme", connection)
            {
                CommandType = CommandType.StoredProcedure, Transaction = transaction
            };

            var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            if ((int)returnParameter.Value != 0)
            {
                transaction.Rollback();
                throw new Exception("Failed to clean up unused WorkflowProcessSchemes ");
            }

            transaction.Commit();
        }
    }
}
