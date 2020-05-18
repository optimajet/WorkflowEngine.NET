using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessInstance : DbObject<WorkflowProcessInstance>
    {
        static WorkflowProcessInstance()
        {
            DbTableName = "WorkflowProcessInstance";
        }

        public WorkflowProcessInstance()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "ActivityName"},
                new ColumnInfo {Name = "IsDeterminingParametersChanged", Type = SqlDbType.Bit},
                new ColumnInfo {Name = "PreviousActivity"},
                new ColumnInfo {Name = "PreviousActivityForDirect"},
                new ColumnInfo {Name = "PreviousActivityForReverse"},
                new ColumnInfo {Name = "PreviousState"},
                new ColumnInfo {Name = "PreviousStateForDirect"},
                new ColumnInfo {Name = "PreviousStateForReverse"},
                new ColumnInfo {Name = "SchemeId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "StateName"},
                new ColumnInfo {Name = "ParentProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "RootProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "TenantId", Type = SqlDbType.NVarChar, Size=1024},
                new ColumnInfo {Name = "StartingTransition", Type = SqlDbType.NVarChar}
            });
        }

        public string ActivityName { get; set; }
        public Guid Id { get; set; }
        public bool IsDeterminingParametersChanged { get; set; }
        public string PreviousActivity { get; set; }
        public string PreviousActivityForDirect { get; set; }
        public string PreviousActivityForReverse { get; set; }
        public string PreviousState { get; set; }
        public string PreviousStateForDirect { get; set; }
        public string PreviousStateForReverse { get; set; }
        public Guid? SchemeId { get; set; }
        public string StateName { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string TenantId { get; set; }
        public string StartingTransition { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "ActivityName":
                    return ActivityName;
                case "IsDeterminingParametersChanged":
                    return IsDeterminingParametersChanged;
                case "PreviousActivity":
                    return PreviousActivity;
                case "PreviousActivityForDirect":
                    return PreviousActivityForDirect;
                case "PreviousActivityForReverse":
                    return PreviousActivityForReverse;
                case "PreviousState":
                    return PreviousState;
                case "PreviousStateForDirect":
                    return PreviousStateForDirect;
                case "PreviousStateForReverse":
                    return PreviousStateForReverse;
                case "SchemeId":
                    return SchemeId;
                case "StateName":
                    return StateName;
                case "ParentProcessId":
                    return ParentProcessId;
                case "RootProcessId":
                    return RootProcessId;
                case "TenantId":
                    return TenantId;
                case "StartingTransition":
                    return StartingTransition;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = (Guid) value;
                    break;
                case "ActivityName":
                    ActivityName = value as string;
                    break;
                case "IsDeterminingParametersChanged":
                    IsDeterminingParametersChanged = value.ToString() == "1";
                    break;
                case "PreviousActivity":
                    PreviousActivity = value as string;
                    break;
                case "PreviousActivityForDirect":
                    PreviousActivityForDirect = value as string;
                    break;
                case "PreviousActivityForReverse":
                    PreviousActivityForReverse = value as string;
                    break;
                case "PreviousState":
                    PreviousState = value as string;
                    break;
                case "PreviousStateForDirect":
                    PreviousStateForDirect = value as string;
                    break;
                case "PreviousStateForReverse":
                    PreviousStateForReverse = value as string;
                    break;
                case "SchemeId":
                    SchemeId = value as Guid?;
                    break;
                case "StateName":
                    StateName = value as string;
                    break;
                case "ParentProcessId":
                    ParentProcessId = value as Guid?;
                    break;
                case "RootProcessId":
                    RootProcessId = (Guid) value;
                    break;
                case "TenantId":
                    TenantId = value as string;
                    break;
                case "StartingTransition":
                    StartingTransition = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }

        }

        public static WorkflowProcessInstance[] GetInstances(SqlConnection connection, IEnumerable<Guid> ids)
        {
            string selectText = String.Format("SELECT * FROM {0} WHERE [Id] IN ({1})", ObjectName, String.Join(",", ids.Select(x => $"'{x}'")));
            return Select(connection, selectText);
        }

#if !NETCOREAPP || NETCORE2
        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));
            dt.Columns.Add("ActivityName", typeof(string));
            dt.Columns.Add("IsDeterminingParametersChanged", typeof(bool));
            dt.Columns.Add("PreviousActivity", typeof(string));
            dt.Columns.Add("PreviousActivityForDirect", typeof(string));
            dt.Columns.Add("PreviousActivityForReverse", typeof(string));
            dt.Columns.Add("PreviousState", typeof(string));
            dt.Columns.Add("PreviousStateForDirect", typeof(string));
            dt.Columns.Add("PreviousStateForReverse", typeof(string));
            dt.Columns.Add("SchemeId", typeof(Guid));
            dt.Columns.Add("StateName", typeof(string));
            dt.Columns.Add("ParentProcessId", typeof(Guid));
            dt.Columns.Add("RootProcessId", typeof(Guid));
            dt.Columns.Add("TenantId", typeof(string));
            dt.Columns.Add("StartingTransition", typeof(string));
            return dt;
        }
#endif
    }
}
