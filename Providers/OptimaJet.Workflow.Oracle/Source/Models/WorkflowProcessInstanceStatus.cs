using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstanceStatus : DbObject<ProcessInstanceStatusEntity>
    {
        public WorkflowProcessInstanceStatus(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstanceS", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = "LOCKFLAG", Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Status), Type = OracleDbType.Int16},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.RuntimeId), Type = OracleDbType.NVarchar2},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.SetTime), Type = OracleDbType.Date}
            });
        }

        public async Task<List<Guid>> GetProcessesByStatusAsync(OracleConnection connection, byte status, string runtimeId = null)
        {
            string command = $"SELECT ID FROM {ObjectName} " + 
                             $"WHERE {nameof(ProcessInstanceStatusEntity.Status).ToUpperInvariant()} = :status";
            
            var p = new List<OracleParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += $" AND {nameof(ProcessInstanceStatusEntity.RuntimeId).ToUpperInvariant()} = :runtime";
                p.Add(new OracleParameter("runtime", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input));
            }

            p.Add(new OracleParameter("status", OracleDbType.Int16, status, ParameterDirection.Input));
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        public async Task<int> ChangeStatusAsync(OracleConnection connection, ProcessInstanceStatusEntity status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"{nameof(ProcessInstanceStatusEntity.Status)} = :newstatus, " + 
                             $"LOCKFLAG = :newlock, " + 
                             $"{nameof(ProcessInstanceStatusEntity.SetTime)} = :settime, " + 
                             $"{nameof(ProcessInstanceStatusEntity.RuntimeId).ToUpperInvariant()} = :runtimeid " + 
                             $"WHERE {nameof(ProcessInstanceStatusEntity.Id)} = :id " + 
                             $"AND LOCKFLAG = :oldlock";
            
            var p1 = new OracleParameter("newstatus", OracleDbType.Int16, status.Status, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, status.Lock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.Raw, status.Id.ToByteArray(), ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);
            var p5 = new OracleParameter("settime", OracleDbType.Date, status.SetTime, ParameterDirection.Input);
            var p6 = new OracleParameter("runtimeid", OracleDbType.NVarchar2, status.RuntimeId, ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
    }
}
