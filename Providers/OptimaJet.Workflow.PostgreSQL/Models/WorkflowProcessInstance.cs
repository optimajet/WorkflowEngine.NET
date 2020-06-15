using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessInstance: DbObject<WorkflowProcessInstance>
    {
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

        static WorkflowProcessInstance()
        {
            DbTableName = "WorkflowProcessInstance";
        }

        public WorkflowProcessInstance()
        {
           DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="ActivityName"},
                new ColumnInfo {Name="IsDeterminingParametersChanged", Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name="PreviousActivity"},
                new ColumnInfo {Name="PreviousActivityForDirect"},
                new ColumnInfo {Name="PreviousActivityForReverse"},
                new ColumnInfo {Name="PreviousState"},
                new ColumnInfo {Name="PreviousStateForDirect"},
                new ColumnInfo {Name="PreviousStateForReverse"},
                new ColumnInfo {Name="SchemeId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="StateName"},
                new ColumnInfo {Name = "ParentProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = "RootProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = "TenantId", Size=1024},
                new ColumnInfo {Name=nameof(StartingTransition)}
            });
        }

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
                case nameof(StartingTransition):
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
                    Id = (Guid)value;
                    break;
                case "ActivityName":
                    ActivityName = value as string;
                    break;
                case "IsDeterminingParametersChanged":
                    IsDeterminingParametersChanged = (bool)value;
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
                    if (value is Guid)
                        SchemeId = (Guid)value;
                    else
                        SchemeId = null;
                    break;
                case "StateName":
                    StateName = value as string;
                    break;
                case "ParentProcessId":
                    if (value is Guid)
                    {
                        ParentProcessId = (Guid) value;
                    }
                    else
                    {
                        ParentProcessId = null;
                    }
                    break;
                case "RootProcessId":
                    RootProcessId = (Guid) value;
                    break;
                case "TenantId":
                    TenantId = value as string;
                    break;
                case nameof(StartingTransition):
                    StartingTransition = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static WorkflowProcessInstance[] GetInstances(NpgsqlConnection connection, IEnumerable<Guid> ids)
        {
            string selectText = String.Format("SELECT * FROM {0} WHERE \"Id\" IN ({1})", ObjectName, String.Join(",", ids.Select(x => $"'{x}'")));
            return Select(connection, selectText);
        }
    }
}
