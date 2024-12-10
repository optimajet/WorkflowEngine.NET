using System;
using System.Data;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessScheme : DbObject<ProcessSchemeEntity>
    {
        public WorkflowProcessScheme(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessScheme", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.DefiningParameters)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.DefiningParametersHash)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.IsObsolete), Type = OracleDbType.Byte},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.SchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.Scheme)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.RootSchemeCode)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.AllowedActivities)},
                new ColumnInfo {Name = nameof(ProcessSchemeEntity.StartingTransition)}
            });
        }

        public async Task<ProcessSchemeEntity[]> SelectAsync(OracleConnection connection, string schemeCode,
            string definingParametersHash, bool? isObsolete, Guid? rootSchemeId)
        {
            string selectText =
                $"SELECT * FROM {ObjectName} " +
                $"WHERE {nameof(ProcessSchemeEntity.SchemeCode)} = :schemecode " +
                $"AND {nameof(ProcessSchemeEntity.DefiningParametersHash)} = :dphash";

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += $" AND {nameof(ProcessSchemeEntity.IsObsolete).ToUpperInvariant()} = 1";
                }
                else
                {
                    selectText += $" AND {nameof(ProcessSchemeEntity.IsObsolete).ToUpperInvariant()} = 0";
                }
            }

            if (rootSchemeId.HasValue)
            {
                selectText += $" AND {nameof(ProcessSchemeEntity.RootSchemeId)} = :rootschemeid";
                return await SelectAsync(connection, selectText,
                        new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                        new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash,
                            ParameterDirection.Input),
                        new OracleParameter("rootschemeid", OracleDbType.Raw, rootSchemeId.Value.ToByteArray(), ParameterDirection.Input))
                    .ConfigureAwait(false);
            }

            selectText += $" AND {nameof(ProcessSchemeEntity.RootSchemeId)} IS NULL";
            return await SelectAsync(connection, selectText,
                    new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                    new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash,
                        ParameterDirection.Input))
                .ConfigureAwait(false);
        }

        public async Task<int> SetObsoleteAsync(OracleConnection connection, string schemeCode)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"{nameof(ProcessSchemeEntity.IsObsolete)} = 1 " +
                             $"WHERE {nameof(ProcessSchemeEntity.SchemeCode)} = :schemecode " +
                             $"OR {nameof(ProcessSchemeEntity.RootSchemeCode)} = :schemecode";
            
            return await ExecuteCommandNonQueryAsync(connection, command,
                new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input)).ConfigureAwait(false);
        }

        public async Task<int> SetObsoleteAsync(OracleConnection connection, string schemeCode, string definingParametersHash)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"{nameof(ProcessSchemeEntity.IsObsolete)} = 1 " +
                             $"WHERE ({nameof(ProcessSchemeEntity.SchemeCode)} = :schemecode " +
                             $"OR {nameof(ProcessSchemeEntity.RootSchemeCode)} = :schemecode) " +
                             $"AND DefiningParametersHash = :dphash";

            return await ExecuteCommandNonQueryAsync(connection, command,
                new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash, ParameterDirection.Input)).ConfigureAwait(false);
        }
        
        public static async Task DeleteUnusedAsync(OracleConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using var transaction = connection.BeginTransaction();
            using var cmd = new OracleCommand("DropUnusedWorkflowProcessScheme", connection)
            {
                CommandType = CommandType.StoredProcedure,
                Transaction = transaction
            };
            var returnParameter = cmd.Parameters.Add("ReturnVal", OracleDbType.Int32, ParameterDirection.ReturnValue);

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            
            if ((OracleDecimal)returnParameter.Value != 0)
            {
                transaction.Rollback();
                throw new Exception("Failed to clean up unused WorkflowProcessSchemes ");
            }
            transaction.Commit();
        }
    }
}
