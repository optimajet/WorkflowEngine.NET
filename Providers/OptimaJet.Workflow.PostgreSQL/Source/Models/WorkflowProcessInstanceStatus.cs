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
    public class WorkflowProcessInstanceStatus : DbObject<ProcessInstanceStatusEntity>
    {
        public WorkflowProcessInstanceStatus(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstanceStatus", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Lock), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Status), Type = NpgsqlDbType.Smallint},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.RuntimeId)},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.SetTime), Type = NpgsqlDbType.Timestamp}
            });
        }

        public async Task<List<Guid>> GetProcessesByStatusAsync(NpgsqlConnection connection, byte status, string runtimeId = null)
        {
            string command = $"SELECT \"{nameof(ProcessInstanceStatusEntity.Id)}\" " + 
                             $"FROM {ObjectName} WHERE \"{nameof(ProcessInstanceStatusEntity.Status)}\" = @status";

            var p = new List<NpgsqlParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += $" AND \"{nameof(ProcessInstanceStatusEntity.RuntimeId)}\" = @runtime";
                p.Add(new NpgsqlParameter("runtime", NpgsqlDbType.Varchar) { Value = runtimeId });
            }

            p.Add(new NpgsqlParameter("status", NpgsqlDbType.Smallint) { Value = status });
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        public async Task<int> ChangeStatusAsync(NpgsqlConnection connection, ProcessInstanceStatusEntity status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"\"{nameof(ProcessInstanceStatusEntity.Status)}\" = @newstatus, " + 
                             $"\"{nameof(ProcessInstanceStatusEntity.Lock)}\" = @newlock, \"SetTime\" = @settime, " + 
                             $"\"{nameof(ProcessInstanceStatusEntity.RuntimeId)}\" = @runtimeid " +
                             $"WHERE \"{nameof(ProcessInstanceStatusEntity.Id)}\" = @id " + 
                             $"AND \"{nameof(ProcessInstanceStatusEntity.Lock)}\" = @oldlock";
            var p1 = new NpgsqlParameter("newstatus", NpgsqlDbType.Smallint) { Value = status.Status };
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = status.Lock };
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = status.Id };
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };
            var p5 = new NpgsqlParameter("settime", NpgsqlDbType.Timestamp) { Value = status.SetTime };
            var p6 = new NpgsqlParameter("runtimeid", NpgsqlDbType.Varchar) { Value = status.RuntimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
    }
}
