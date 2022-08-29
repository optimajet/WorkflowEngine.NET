using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessInstancePersistence : DbObject<ProcessInstancePersistenceEntity>
    {
        public WorkflowProcessInstancePersistence(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstancePersistence", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ProcessId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ParameterName)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Value), Type = NpgsqlDbType.Text}
            });
        }

        public async Task<ProcessInstancePersistenceEntity[]> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid";
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        public async Task<ProcessInstancePersistenceEntity> SelectByNameAsync(NpgsqlConnection connection, Guid processId,  string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid " + 
                                $"AND \"{nameof(ProcessInstancePersistenceEntity.ParameterName)}\" = @parameterName";

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId}, 
                new NpgsqlParameter("parameterName", NpgsqlDbType.Text) {Value = parameterName}
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid",
                    transaction,
                    p)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByNameAsync(NpgsqlConnection connection, Guid processId, string parameterName, NpgsqlTransaction transaction = null)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId},
                new NpgsqlParameter("parameterName", NpgsqlDbType.Text) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid " +
                    $"AND \"{nameof(ProcessInstancePersistenceEntity.ParameterName)}\" = @parameterName",
                    transaction,
                    parameters.ToArray())
                .ConfigureAwait(false);
        }
    }
}
