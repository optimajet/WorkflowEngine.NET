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
    public class WorkflowProcessInstance : DbObject<ProcessInstanceEntity>
    {
        public WorkflowProcessInstance(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessInstance", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ActivityName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.IsDeterminingParametersChanged), Type = SqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivity)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousState)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SchemeId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StateName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ParentProcessId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.RootProcessId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.TenantId), Type = SqlDbType.NVarChar, Size=1024},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StartingTransition), Type = SqlDbType.NVarChar},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SubprocessName), Type = SqlDbType.NVarChar},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CreationDate), Type = SqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.LastTransitionDate), Type = SqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CalendarName), Type = SqlDbType.NVarChar}
            });
        }

        public async Task<ProcessInstanceEntity[]> GetInstances(SqlConnection connection, IEnumerable<Guid> ids)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE [{nameof(ProcessInstanceEntity.Id)}] IN ({String.Join(",", ids.Select(x => $"'{x}'"))})";
            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }

        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add(nameof(ProcessInstanceEntity.Id), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstanceEntity.ActivityName), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.IsDeterminingParametersChanged), typeof(bool));
            dt.Columns.Add(nameof(ProcessInstanceEntity.PreviousActivity), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.PreviousActivityForDirect), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.PreviousActivityForReverse), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.PreviousState), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.PreviousStateForDirect), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.PreviousStateForReverse), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.SchemeId), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstanceEntity.StateName), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.ParentProcessId), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstanceEntity.RootProcessId), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstanceEntity.TenantId), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.StartingTransition), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.SubprocessName), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.CreationDate), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.LastTransitionDate), typeof(string));
            dt.Columns.Add(nameof(ProcessInstanceEntity.CalendarName), typeof(string));
            return dt;
        }
    }
}
