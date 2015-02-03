using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;

namespace OptimaJet.Workflow.Oracle
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

        private static string _tableName = "WorkflowProcessTransitionHistory";

        public WorkflowProcessTransitionHistory()
            : base()
        {
            db_TableName = _tableName;
            db_Columns.AddRange(new ColumnInfo[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo(){Name="ActorIdentityId"},
                new ColumnInfo(){Name="ExecutorIdentityId"},
                new ColumnInfo(){Name="FromActivityName"},
                new ColumnInfo(){Name="FromStateName"},
                new ColumnInfo(){Name="IsFinalised", Type = NpgsqlDbType.Boolean},
                new ColumnInfo(){Name="ProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo(){Name="ToActivityName"},
                new ColumnInfo(){Name="ToStateName"},
                new ColumnInfo(){Name="TransitionClassifier"},
                new ColumnInfo(){Name="TransitionTime", Type = NpgsqlDbType.Date },
                new ColumnInfo(){Name="TriggerName"}
            });
        }

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
                    Id = (Guid)value;
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
                    IsFinalised = (bool)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
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

        public static int DeleteByProcessId(NpgsqlConnection connection, Guid processId)
        {
            var p_processId = new NpgsqlParameter("processId", NpgsqlDbType.Uuid);
            p_processId.Value = processId;
            return ExecuteCommand(connection, string.Format("DELETE FROM \"{0}\" WHERE \"ProcessId\" = @processid", _tableName), p_processId);
        }
    }
}
