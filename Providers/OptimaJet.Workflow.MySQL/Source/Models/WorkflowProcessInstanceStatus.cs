using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessInstanceStatus : DbObject<ProcessInstanceStatusEntity>
    {
        public WorkflowProcessInstanceStatus(int commandTimeout) : base("workflowprocessinstancestatus", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Lock), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Status), Type = MySqlDbType.Byte},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.RuntimeId), Type = MySqlDbType.VarChar},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.SetTime), Type = MySqlDbType.DateTime}
            });
        }

        public async Task<List<Guid>> GetProcessesByStatusAsync(MySqlConnection connection, byte status, string runtimeId = null)
        {
            string command = $"SELECT `{nameof(ProcessInstanceStatusEntity.Id)}` FROM {DbTableName} " + 
                             $"WHERE `{nameof(ProcessInstanceStatusEntity.Status)}` = @status";
            
            var p = new List<MySqlParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += $" AND `{nameof(ProcessInstanceStatusEntity.RuntimeId)}` = @runtime";
                p.Add(new MySqlParameter("runtime", MySqlDbType.VarChar) { Value = runtimeId });
            }

            p.Add(new MySqlParameter("status", MySqlDbType.Byte)
            {
                Value = ToDbValue(status, MySqlDbType.Byte)
            });
            
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        public async Task<int> ChangeStatusAsync(MySqlConnection connection, ProcessInstanceStatusEntity status, Guid oldLock)
        {
            string command = $"UPDATE {DbTableName} SET " + 
                             $"`{nameof(ProcessInstanceStatusEntity.Status)}` = @newstatus, " + 
                             $"`{nameof(ProcessInstanceStatusEntity.Lock)}` = @newlock, " + 
                             $"`{nameof(ProcessInstanceStatusEntity.SetTime)}` = @settime, " + 
                             $"`{nameof(ProcessInstanceStatusEntity.RuntimeId)}` = @runtimeid " + 
                             $"WHERE `{nameof(ProcessInstanceStatusEntity.Id)}` = @id " + 
                             $"AND `{nameof(ProcessInstanceStatusEntity.Lock)}` = @oldlock";
            
            var p1 = new MySqlParameter("newstatus", MySqlDbType.Byte) 
            {
                Value = ToDbValue(status.Status, MySqlDbType.Byte)
            };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = status.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.Binary) { Value = status.Id.ToByteArray() };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };
            var p5 = new MySqlParameter("settime", MySqlDbType.DateTime) { Value = status.SetTime };
            var p6 = new MySqlParameter("runtimeid", MySqlDbType.VarChar) { Value = status.RuntimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
    }
}
