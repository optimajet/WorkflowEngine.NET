using System;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessTransitionHistory : DbObject<WorkflowProcessTransitionHistory>
    {
        public string ActorIdentityId { get; set; }
        public string ExecutorIdentityId { get; set; }
        public string FromActivityName { get; set; }
        public string FromStateName { get; set; }
        public Guid Id { get; set; }
        public bool IsFinalised { get; set; }
        public Guid ProcessId { get; set; }
        public string ToActivityName { get; set; }
        public string ToStateName { get; set; }
        public string TransitionClassifier { get; set; }
        public DateTime TransitionTime { get; set; }

        public string TriggerName { get; set; }

        static WorkflowProcessTransitionHistory()
        {
            DbTableName = "workflowprocesstransitionhistory";
        }

       public WorkflowProcessTransitionHistory()
       {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name="ActorIdentityId"},
                new ColumnInfo {Name="ExecutorIdentityId"},
                new ColumnInfo {Name="FromActivityName"},
                new ColumnInfo {Name="FromStateName"},
                new ColumnInfo {Name="IsFinalised", Type = MySqlDbType.Byte},
                new ColumnInfo {Name="ProcessId", Type = MySqlDbType.Binary},
                new ColumnInfo {Name="ToActivityName"},
                new ColumnInfo {Name="ToStateName"},
                new ColumnInfo {Name="TransitionClassifier"},
                new ColumnInfo {Name="TransitionTime", Type = MySqlDbType.DateTime },
                new ColumnInfo {Name="TriggerName"}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "ActorIdentityId":
                    return ActorIdentityId;
                case "ExecutorIdentityId":
                    return ExecutorIdentityId;
                case "FromActivityName":
                    return FromActivityName;
                case "FromStateName":
                    return FromStateName;
                case "IsFinalised":
                    return IsFinalised;
                case "ProcessId":
                    return ProcessId.ToByteArray();
                case "ToActivityName":
                    return ToActivityName;
                case "ToStateName":
                    return ToStateName;
                case "TransitionClassifier":
                    return TransitionClassifier;
                case "TransitionTime":
                    return TransitionTime;
                case "TriggerName":
                    return TriggerName;
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
                case "ActorIdentityId":
                    ActorIdentityId = value as string;
                    break;
                case "ExecutorIdentityId":
                    ExecutorIdentityId = value as string;
                    break;
                case "FromActivityName":
                    FromActivityName = value as string;
                    break;
                case "FromStateName":
                    FromStateName = value as string;
                    break;
                case "IsFinalised":
                    IsFinalised = value.ToString() == "1";
                    break;
                case "ProcessId":
                    ProcessId = new Guid((byte[])value);
                    break;
                case "ToActivityName":
                    ToActivityName = value as string;
                    break;
                case "ToStateName":
                    ToStateName = value as string;
                    break;
                case "TransitionClassifier":
                    TransitionClassifier = value as string;
                    break;
                case "TransitionTime":
                    TransitionTime = (DateTime)value;
                    break;
                case "TriggerName":
                    TriggerName = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int DeleteByProcessId(MySqlConnection connection, Guid processId,
            MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };
            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE `ProcessId` = @processid", DbTableName), transaction, pProcessId);
        }

        public static WorkflowProcessTransitionHistory[] SelectByProcessId(MySqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE `ProcessId` = @processid", DbTableName);

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            return Select(connection, selectText, p1);
        }
    }
}
