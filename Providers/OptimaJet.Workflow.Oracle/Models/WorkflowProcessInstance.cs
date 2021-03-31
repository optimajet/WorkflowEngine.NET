
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
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
        public string SubprocessName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastTransitionDate { get; set; }

        static WorkflowProcessInstance()
        {
            DbTableName = "WorkflowProcessInstance";
        }

        public WorkflowProcessInstance()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name="ActivityName"},
                new ColumnInfo {Name="IsDeterminingParametersChanged", Type = OracleDbType.Byte},
                new ColumnInfo {Name="PreviousActivity"},
                new ColumnInfo {Name="PreviousActivityForDirect"},
                new ColumnInfo {Name="PreviousActivityForReverse"},
                new ColumnInfo {Name="PreviousState"},
                new ColumnInfo {Name="PreviousStateForDirect"},
                new ColumnInfo {Name="PreviousStateForReverse"},
                new ColumnInfo {Name="SchemeId", Type = OracleDbType.Raw},
                new ColumnInfo {Name="StateName"},
                new ColumnInfo {Name = "ParentProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "RootProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "TenantId", Size=1024},
                new ColumnInfo {Name=nameof(StartingTransition)},
                new ColumnInfo {Name=nameof(SubprocessName)},
                new ColumnInfo {Name = "CreationDate", Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = "LastTransitionDate", Type = OracleDbType.TimeStamp}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "ActivityName":
                    return ActivityName;
                case "IsDeterminingParametersChanged":
                    return IsDeterminingParametersChanged ? "1" : "0";
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
                    return SchemeId.HasValue ? SchemeId.Value.ToByteArray() : null;
                case "StateName":
                    return StateName;
                case "ParentProcessId":
                    return ParentProcessId.HasValue ? ParentProcessId.Value.ToByteArray() : null;
                case "RootProcessId":
                    return RootProcessId.ToByteArray();
                case "TenantId":
                    return TenantId;
                case nameof(StartingTransition):
                    return StartingTransition;
                case nameof(SubprocessName):
                    return SubprocessName;
                case "CreationDate":
                    return CreationDate;
                case "LastTransitionDate":
                    return LastTransitionDate;
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
                case "ActivityName":
                    ActivityName = value as string;
                    break;
                case "IsDeterminingParametersChanged":
                    IsDeterminingParametersChanged = (string)value == "1";
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
                    if (value is byte[])
                        SchemeId = new Guid((byte[])value);
                    else
                        SchemeId = null;
                    break;
                case "StateName":
                    StateName = value as string;
                    break;
                case "ParentProcessId":
                    var bytes1 = value as byte[];
                    if (bytes1 != null)
                        ParentProcessId = new Guid(bytes1);
                    else
                        ParentProcessId = null;
                    break;
                case "RootProcessId":
                    RootProcessId = new Guid((byte[])value);
                    break;
                case "TenantId":
                    TenantId = value as string;
                    break;
                case nameof(StartingTransition):
                    StartingTransition = value as string;
                    break;
                case nameof(SubprocessName):
                    SubprocessName = value as string;
                    break;
                case "CreationDate":
                    CreationDate = (DateTime)value;
                    break;
                case "LastTransitionDate":
                    LastTransitionDate = value as DateTime?;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static async Task<WorkflowProcessInstance[]> GetInstances(OracleConnection connection, IEnumerable<Guid> ids)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE ID IN ({String.Join(",", ids.Select(x => $"HEXTORAW('{BitConverter.ToString(x.ToByteArray()).Replace("-", String.Empty)}')"))})";
            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }
    }
}
