using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace OptimaJet.Workflow.PostgreSQL
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
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="ProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="IdentityId"},
                new ColumnInfo {Name="AllowedTo"},
                new ColumnInfo {Name="TransitionTime", Type = NpgsqlDbType.Date},
                new ColumnInfo {Name="Sort", Type = NpgsqlDbType.Bigint},
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
                    return Id;
                case "ProcessId":
                    return ProcessId;
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
                    Id = (Guid)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
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
                    Sort = (long)value;
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

        public static WorkflowApprovalHistory[] SelectByProcessId(NpgsqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE  \"ProcessId\" = @processId", ObjectName);

            var processIdParameter = new NpgsqlParameter("processId", NpgsqlDbType.Uuid) { Value = processId };

            return Select(connection, selectText, processIdParameter);
        }
    }
}
