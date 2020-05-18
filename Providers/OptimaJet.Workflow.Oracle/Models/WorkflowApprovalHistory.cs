using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowApprovalHistory : DbObject<WorkflowApprovalHistory>
    {

        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public string AllowedTo { get; set; }
        public DateTime? TransitionTime { get; set; }
        public long Sort { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string TriggerName { get; set; }
        public string Commentary { get; set; }

        static WorkflowApprovalHistory()
        {
            DbTableName = "WorkflowApprovalHistory";
        }
       
       public WorkflowApprovalHistory()
       {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name="ProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name="IdentityId"},
                new ColumnInfo {Name="AllowedTo"},
                new ColumnInfo {Name="TransitionTime", Type = OracleDbType.Date},
                new ColumnInfo {Name="Sort", Type = OracleDbType.Int64},
                new ColumnInfo {Name="InitialState"},
                new ColumnInfo {Name="DestinationState"},
                new ColumnInfo {Name="TriggerName"},
                new ColumnInfo {Name="Commentary"},
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "ProcessId":
                    return ProcessId.ToByteArray();
                case "IdentityId":
                    return IdentityId;
                case "AllowedTo":
                    return AllowedTo;
                case "TransitionTime":
                    return TransitionTime;
                case "Sort":
                    return Sort;
                case "InitialState":
                    return InitialState;
                case "DestinationState":
                    return DestinationState;
                case "TriggerName":
                    return TriggerName;
                case "Commentary":
                    return Commentary;
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
                case "ProcessId":
                    ProcessId = new Guid((byte[])value);
                    break;
                case "IdentityId":
                    IdentityId = value as string;
                break;
                case "AllowedTo":
                    AllowedTo = value as string;
                    break;
                case "TransitionTime":
                {
                    TransitionTime = null;
                    if (value != null)
                    {
                        TransitionTime = (DateTime?)value;
                    }

                }
                break;
                case "Sort":
                    Sort = (long)(decimal)value;
                    break;
                case "InitialState":
                    InitialState = value as string;
                    break;
                case "DestinationState":
                    DestinationState = value as string;
                    break;
                case "TriggerName":
                    TriggerName = value as string;
                    break;
                case "Commentary":
                    Commentary = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowApprovalHistory[] SelectByProcessId(OracleConnection connection, Guid processId)
        {
            string name = "ProcessId";

            var selectText = string.Format("SELECT * FROM {0} WHERE  {1} = :processId", ObjectName, name.ToUpperInvariant());

            var processIdParameter = new OracleParameter("processId", OracleDbType.Raw) { Value = processId.ToByteArray() };

            return Select(connection, selectText, processIdParameter);
        }
    }
}
