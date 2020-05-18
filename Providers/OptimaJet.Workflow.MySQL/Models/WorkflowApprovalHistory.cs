using System;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
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
            DbTableName = "workflowapprovalhistory";
        }
       
       public WorkflowApprovalHistory()
       {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name="ProcessId", Type = MySqlDbType.Binary},
                new ColumnInfo {Name="IdentityId"},
                new ColumnInfo {Name="AllowedTo"},
                new ColumnInfo {Name="TransitionTime", Type = MySqlDbType.DateTime},
                new ColumnInfo {Name="Sort", Type = MySqlDbType.Int64},
                new ColumnInfo {Name="InitialState", Type = MySqlDbType.Binary},
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

        public static WorkflowApprovalHistory[] SelectByProcessId(MySqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE `ProcessId` = @processId", DbTableName);

            var processIdParameter = new MySqlParameter("processId", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            return Select(connection, selectText, processIdParameter);
        }
    }
}
