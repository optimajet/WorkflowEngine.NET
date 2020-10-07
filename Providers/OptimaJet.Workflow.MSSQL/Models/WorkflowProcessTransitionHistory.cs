using System;
using System.Data;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessTransitionHistory : DbObject<WorkflowProcessTransitionHistory>
    {
        static WorkflowProcessTransitionHistory()
        {
            DbTableName = "WorkflowProcessTransitionHistory";
        }

        public WorkflowProcessTransitionHistory()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "ActorIdentityId"},
                new ColumnInfo {Name = "ExecutorIdentityId"},
                new ColumnInfo {Name = "FromActivityName"},
                new ColumnInfo {Name = "FromStateName"},
                new ColumnInfo {Name = "IsFinalised", Type = SqlDbType.Bit},
                new ColumnInfo {Name = "ProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "ToActivityName"},
                new ColumnInfo {Name = "ToStateName"},
                new ColumnInfo {Name = "TransitionClassifier"},
                new ColumnInfo {Name = "TransitionTime", Type = SqlDbType.DateTime},
                new ColumnInfo {Name = "TriggerName"}
            });
        }

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

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
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
                    return ProcessId;
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
                    Id = (Guid) value;
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
                    IsFinalised = (bool) value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid) value;
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
                    TransitionTime = (DateTime) value;
                    break;
                case "TriggerName":
                    TriggerName = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return await ExecuteCommandAsync(connection, $"DELETE FROM {ObjectName} WHERE [ProcessId] = @processid", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTransitionHistory[]> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [ProcessId] = @processid";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
