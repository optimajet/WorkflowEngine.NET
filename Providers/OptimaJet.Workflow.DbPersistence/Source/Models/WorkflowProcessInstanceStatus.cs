using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessInstanceStatus : DbObject<ProcessInstanceStatusEntity>
    {
        public WorkflowProcessInstanceStatus(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstanceStatus", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Lock), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Status), Type = SqlDbType.TinyInt},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.RuntimeId), Type = SqlDbType.NVarChar},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.SetTime), Type = SqlDbType.DateTime}
            });
        }
        
        public async Task<List<Guid>> GetProcessesByStatusAsync(SqlConnection connection, byte status, string runtimeId)
        {
            string command = $"SELECT [{nameof(ProcessInstanceStatusEntity.Id)}] " + 
                             $"FROM {ObjectName} WHERE [{nameof(ProcessInstanceStatusEntity.Status)}] = @status";
            var p = new List<SqlParameter>(); 

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += $" AND [{nameof(ProcessInstanceStatusEntity.RuntimeId)}] = @runtime";
                p.Add(new SqlParameter("runtime", SqlDbType.NVarChar) { Value = runtimeId });
            }

            p.Add(new SqlParameter("status", SqlDbType.TinyInt) { Value = status });
            
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        public async Task<int> ChangeStatusAsync(SqlConnection connection, ProcessInstanceStatusEntity instanceStatus, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET [{nameof(ProcessInstanceStatusEntity.Status)}] = @newstatus, " +
                             $"[{nameof(ProcessInstanceStatusEntity.Lock)}] = @newlock, " +
                             $"[{nameof(ProcessInstanceStatusEntity.SetTime)}] = @settime, " +
                             $"[{nameof(ProcessInstanceStatusEntity.RuntimeId)}] = @runtimeid " +
                             $"WHERE [{nameof(ProcessInstanceStatusEntity.Id)}] = @id " +
                             $"AND [{nameof(ProcessInstanceStatusEntity.Lock)}] = @oldlock";
            var p1 = new SqlParameter("newstatus", SqlDbType.TinyInt) {Value = instanceStatus.Status};
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) {Value = instanceStatus.Lock};
            var p3 = new SqlParameter("id", SqlDbType.UniqueIdentifier) {Value = instanceStatus.Id};
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) {Value = oldLock};
            var p5 = new SqlParameter("settime", SqlDbType.DateTime) { Value = instanceStatus.SetTime };
            var p6 = new SqlParameter("runtimeid", SqlDbType.NVarChar) { Value = instanceStatus.RuntimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
        
        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add(nameof(ProcessInstanceStatusEntity.Id), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstanceStatusEntity.Lock), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstanceStatusEntity.Status), typeof(byte));
            dt.Columns.Add(nameof(ProcessInstanceStatusEntity.RuntimeId), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceStatusEntity.SetTime), typeof(DateTime));
            return dt;
        }
    }
}
