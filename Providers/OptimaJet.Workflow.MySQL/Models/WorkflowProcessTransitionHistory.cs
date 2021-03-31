using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using OptimaJet.Workflow.Core.Persistence;

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
        public DateTime? StartTransitionTime { get; set; }
        public long? TransitionDuration { get; set; }

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
                new ColumnInfo {Name="TriggerName"},
                new ColumnInfo {Name = "StartTransitionTime", Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = "TransitionDuration", Type = MySqlDbType.Int64},
            });
        }

        public override object GetValue(string key)
        {
            return key switch
            {
                "Id" => Id.ToByteArray(),
                "ActorIdentityId" => ActorIdentityId,
                "ExecutorIdentityId" => ExecutorIdentityId,
                "FromActivityName" => FromActivityName,
                "FromStateName" => FromStateName,
                "IsFinalised" => IsFinalised,
                "ProcessId" => ProcessId.ToByteArray(),
                "ToActivityName" => ToActivityName,
                "ToStateName" => ToStateName,
                "TransitionClassifier" => TransitionClassifier,
                "TransitionTime" => TransitionTime,
                "TriggerName" => TriggerName,
                "StartTransitionTime" => StartTransitionTime,
                "TransitionDuration" => TransitionDuration,
                _ => throw new Exception($"Column {key} is not exists")
            };
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
                case "StartTransitionTime":
                    StartTransitionTime = value as DateTime?;
                    break;
                case "TransitionDuration":
                    TransitionDuration = value as long?;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId,
            MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };
            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processid", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<List<WorkflowProcessTransitionHistory>> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `ProcessId` = @processid";

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            return (await SelectAsync(connection, selectText, p1).ConfigureAwait(false)).ToList();
        }
        
        public static async Task<List<WorkflowProcessTransitionHistory>> SelectByIdentityIdAsync(MySqlConnection connection, string identityId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `ExecutorIdentityId` = @executorIdentityId";
        
            var p1 = new MySqlParameter("executorIdentityId", MySqlDbType.VarString) { Value = identityId };

            return (await SelectAsync(connection, selectText, p1).ConfigureAwait(false)).ToList();
        }
        
    }
}
