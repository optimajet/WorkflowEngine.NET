using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        public Guid Id { get; set; }
        public Guid LOCKFLAG { get; set; }
        public short Status { get; set; }

        public string RuntimeId { get; set; }

        public DateTime SetTime { get; set; }

        static WorkflowProcessInstanceStatus()
        {
            DbTableName = "WorkflowProcessInstanceS";
        }

        public WorkflowProcessInstanceStatus()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = "LOCKFLAG", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "Status", Type = OracleDbType.Int16},
                new ColumnInfo {Name = "RuntimeId", Type = OracleDbType.NVarchar2},
                new ColumnInfo {Name = "SetTime", Type = OracleDbType.Date}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                   return Id.ToByteArray();
                case "LOCKFLAG":
                    return LOCKFLAG.ToByteArray();
                case "Status":
                    return Status;
                case "RuntimeId":
                    return RuntimeId;
                case "SetTime":
                    return SetTime;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }

        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = new Guid((byte[])value);
                    break;
                case "LOCKFLAG":
                    LOCKFLAG = new Guid((byte[])value);
                    break;
                case "Status":
                    Status = (short) value;
                    break;
                case "RuntimeId":
                    RuntimeId = (string)value;
                    break;
                case "SetTime":
                    SetTime = (DateTime)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<List<Guid>> GetProcessesByStatusAsync(OracleConnection connection, byte status, string runtimeId = null)
        {
            string command = $"SELECT ID FROM {ObjectName} WHERE STATUS = :status";
            var p = new List<OracleParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += " AND RUNTIMEID = :runtime";
                p.Add(new OracleParameter("runtime", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input));
            }

            p.Add(new OracleParameter("status", OracleDbType.Int16, status, ParameterDirection.Input));
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        public static async Task<int> ChangeStatusAsync(OracleConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET Status = :newstatus, LOCKFLAG = :newlock, SetTime = :settime, RUNTIMEID = :runtimeid WHERE Id = :id AND LOCKFLAG = :oldlock";
            var p1 = new OracleParameter("newstatus", OracleDbType.Int16, status.Status, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, status.LOCKFLAG.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.Raw, status.Id.ToByteArray(), ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);
            var p5 = new OracleParameter("settime", OracleDbType.Date, status.SetTime, ParameterDirection.Input);
            var p6 = new OracleParameter("runtimeid", OracleDbType.NVarchar2, status.RuntimeId, ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
    }
}
